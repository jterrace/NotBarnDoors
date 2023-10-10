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
            Main.logger.LogInfo("Door component awaked");
            _currentAlt = false;
            _parent = gameObject.GetComponent<Door>();
            _parent.m_nview.Register<int>("SetDoorAutoCloseTime", new Action<long, int>(this.RPC_SetDoorAutoCloseTime));
            if (_parent.m_nview.GetZDO().IsOwner())
            {
                UpdateState();
            }
        }

        public string GetText()
        {
            return _parent.m_nview.GetZDO().GetInt(Main.s_doorAutoCloseTime, 0).ToString();
        }

        public void SetText(string text)
        {
            Main.logger.LogInfo("Set Text: " + text);
            if (text.Length == 0) return;
            if (!Int32.TryParse(text, out int newAutoCloseTime)) return;
            if (newAutoCloseTime < 0 || newAutoCloseTime > 999) return;
            _parent.m_nview.InvokeRPC("SetDoorAutoCloseTime", new object[] { newAutoCloseTime });
        }

        public void AltOn()
        {
            _currentAlt = true;
        }
        public void AltOff()
        {
            _currentAlt = false;
        }

        public void UpdateState()
        {
            Main.logger.LogInfo("UpdateState");
            int closeTime = _parent.m_nview.GetZDO().GetInt(Main.s_doorAutoCloseTime, 0);
            Main.logger.LogInfo("UpdateState closeTime = " + closeTime);
            CancelInvoke(nameof(CloseTheDamnDoor));
            if (closeTime <= 0) return;
            int doorState = _parent.m_nview.GetZDO().GetInt(ZDOVars.s_state, 0);
            Main.logger.LogInfo("UpdateState doorState = " + doorState);
            if (doorState == 0) return;
            Invoke(nameof(CloseTheDamnDoor), closeTime);
        }

        public void CloseTheDamnDoor()
        {
            Main.logger.LogInfo("CloseTheDamnDoor");
            ZDO zdo = _parent.m_nview.GetZDO();
            if (!zdo.IsOwner()) return;
            int doorState = zdo.GetInt(ZDOVars.s_state, 0);
            Main.logger.LogInfo("CloseTheDamnDoor doorState = " + doorState);
            if (doorState != 0)
            {
                _parent.RPC_UseDoor(0, true);
            }
        }

        public void RPC_SetDoorAutoCloseTime(long uid, int autoCloseTime)
        {
            _parent.m_nview.GetZDO().Set(Main.s_doorAutoCloseTime, autoCloseTime, false);
            UpdateState();
        }
    }
}
