using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NotBarnDoors
{
    public class DoorComponent : MonoBehaviour, TextReceiver
    {
        private bool _currentAlt;
        private Door _parent;

        public bool Alt => _currentAlt;

        private void Awake() {
            _currentAlt = false;
            _parent = gameObject.GetComponent<Door>();
            _parent.m_nview.Register<int>("SetDoorAutoCloseTime", new Action<long, int>(this.RPC_SetDoorAutoCloseTime));
            _parent.m_nview.Register<int>("CloseTheDoorLater", new Action<long, int>(this.RPC_CloseTheDoorLater));
            CheckDoorNeedsClosing();
        }

        public string GetText()
        {
            return _parent.m_nview.GetZDO().GetInt(Main.s_doorAutoCloseTime, 0).ToString();
        }

        public void SetText(string text)
        {
            if (text.Length == 0) return;
            if (!Int32.TryParse(text, out int newAutoCloseTime)) return;
            if (newAutoCloseTime < 0 || newAutoCloseTime > 999) return;
            _parent.m_nview.InvokeRPC("SetDoorAutoCloseTime", new object[] { newAutoCloseTime });
        }

        public void RPC_SetDoorAutoCloseTime(long uid, int autoCloseTime)
        {
            _parent.m_nview.GetZDO().Set(Main.s_doorAutoCloseTime, autoCloseTime, false);
            CheckDoorNeedsClosing();
        }

        public void AltOn()
        {
            _currentAlt = true;
        }
        public void AltOff()
        {
            _currentAlt = false;
        }

        public void CheckDoorNeedsClosing()
        {
            int closeTime = _parent.m_nview.GetZDO().GetInt(Main.s_doorAutoCloseTime, 0);
            if (closeTime <= 0) return;
            int doorState = _parent.m_nview.GetZDO().GetInt(ZDOVars.s_state, 0);
            if (doorState == 0) return;
            if (_parent.m_nview.GetZDO().IsOwner())
            {
                // Owner sends to everyone in case they are the owner at trigger time.
                _parent.m_nview.InvokeRPC(ZNetView.Everybody, "CloseTheDoorLater", new object[] { closeTime });
            } else
            {
                // If not the owner, just set up invoke locally.
                RPC_CloseTheDoorLater(0, closeTime);
            }
        }

        public void RPC_CloseTheDoorLater(long uid, int closeAfterSeconds)
        {
            CancelInvoke(nameof(CloseTheDamnDoor));
            if (closeAfterSeconds <= 0) return;
            Invoke(nameof(CloseTheDamnDoor), closeAfterSeconds);
        }

        public void CloseTheDamnDoor()
        {
            ZDO zdo = _parent.m_nview.GetZDO();
            if (!zdo.IsOwner()) return;
            int doorState = zdo.GetInt(ZDOVars.s_state, 0);
            if (doorState == 0) return;
            int closeTime = _parent.m_nview.GetZDO().GetInt(Main.s_doorAutoCloseTime, 0);
            if (closeTime <= 0) return;
            Main.logger.LogInfo("Closing door");
            _parent.RPC_UseDoor(0, true);
        }
    }
}
