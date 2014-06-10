using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTTP_Proxy
{
    internal enum States
    {

    }

    class Automata
    {
        public delegate void OnStateAct(int position, string body);

        protected Dictionary<States, OnStateAct> ActionsOnState;
        protected Dictionary<char, Dictionary<States, States>> TransitionTable;
        protected List<States> EndStates;
        protected States BeginState;

        protected States State;

        private void MakeAct(int position, string body)
        {
            if (ActionsOnState.ContainsKey(State))
            {
                ActionsOnState[State](position,body);
            }
        }
        
        public Dictionary<string, string> AutomataGo(string target)
        {
            var answer = new Dictionary<string, string>();

            State = BeginState;

            var body = String.Empty;
            var Token = String.Empty;

            MakeAct(-1,body);

            for (int i = 0; i < target.Length; i++)
            {
                body += target.ElementAt(i);
                if ()
                MakeAct(i,body);
            }
        }    
    }
}
