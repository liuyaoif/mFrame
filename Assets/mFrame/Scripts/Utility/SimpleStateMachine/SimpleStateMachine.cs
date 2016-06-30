using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Utility
{
    public interface ISimpleState
    {
        int StateId { set; get; }
        void Enter();
        void Update();
        void Leave();
        void Dispose();

        void SetOwnerMachine(SimpleStateMachine owner);
    }

    public class SimpleStateMachine
    {
        private Dictionary<int, ISimpleState> m_stateDict = new Dictionary<int, ISimpleState>();
        private ISimpleState m_lastState;
        public ISimpleState LastState
        {
            get { return m_lastState; }
        }

        private ISimpleState m_curState;
        public ISimpleState CurState
        {
            get { return m_curState; }
        }

        public ISimpleState AddState(ISimpleState state)
        {
            m_stateDict.Add(state.StateId, state);
            state.SetOwnerMachine(this);
            return state;
        }

        public void RemoveState(int stateId)
        {
            m_stateDict.Remove(stateId);
        }

        public void ChangeState(int stateId)
        {
            if (!m_stateDict.ContainsKey(stateId))
            {
                LogManager.Instance.LogError(UtilTools.CombineString("No such state: ", stateId));
                return;
            }

            if (m_curState != null)
            {
                m_curState.Leave();
            }

            m_lastState = m_curState;
            m_curState = m_stateDict[stateId];
            m_curState.Enter();
        }

        public void Update()
        {
            if (m_curState != null)
            {
                m_curState.Update();
            }
        }

        public void Dispose()
        {
            foreach (var kvp in m_stateDict)
            {
                kvp.Value.Dispose();
            }
            m_stateDict.Clear();
        }
    }//SimpleStateMachine
}
