using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proto_Parser;

namespace HTTP_Proxy
{
    internal enum States
    {
        Nothing
    }

    internal abstract class Automata
    {
        public delegate void OnStateAct(int position, string body);
        public delegate bool TransitNeed(char sym);

        private Dictionary<States, OnStateAct> onStateActs = new Dictionary<States, OnStateAct>(); 

        private Dictionary<States, Dictionary<TransitNeed, States>> transitionTable =
            new Dictionary<States, Dictionary<TransitNeed, States>>();

        private HashSet<States> finalStates = new HashSet<States>();
        private Dictionary<States, States> defaultTransitions = new Dictionary<States, States>();
        
        private States initState;
        private String tokenType;
        private int numberOfWord;

        public Token TryReadToken(String input, int offset)
        {
            var currentState = initState;
            var index = offset;
            var finalStateIndex = offset;

            var nextState = NextState(currentState, input.ElementAt(index));
            while (index < input.Length && nextState != null)
            {
                currentState = nextState;
                index++;

                if (finalStates.Contains(currentState))
                    finalStateIndex = index;

                nextState = NextState(currentState, input.ElementAt(index));

                if (onStateActs.ContainsKey(currentState))
                {
                    onStateActs[currentState](index, input.Substring(offset, finalStateIndex));
                }
            }

            if (finalStateIndex == offset)
                return null;

            String text = input.Substring(offset, finalStateIndex);
            return new Token(tokenType, text, GetTokenValue(text));
        }

        public Token TryReadToken(String input)
        {
            return this.TryReadToken(input, 0);
        }

        /**
         * Вычисляет следующее состояние для автомата
         * 
         * @param state
         *            текущее состояние
         * @param symbol
         *            очередной символ
         * @return следующее состояние
         */

        private States NextState(States state, char symbol)
        {
            Dictionary<char, States> currentState;
            States nextState;
            if ((currentState = transitionTable[state]) != null
                && (nextState = currentState[symbol]) != null)
                return nextState;

            States defaultTransition;
            if ((defaultTransition = defaultTransitions[state]) != null)
                return defaultTransition;

            return States.Nothing;
        }

        /**
         * Вычисляет значение токена из полученной строки
         * 
         * @param text
         * @return значение токена
         */
        protected abstract Object GetTokenValue(String text);

        /**
         * Устанавливает тип токена
         * 
         * @param type
         */

        protected void SetTokenType(String type)
        {
            tokenType = type;
        }

        /**
         * Добавляет переход в таблицу.
         * 
         * @param from
         *            текущее состояние
         * @param symbol
         *            считанный символ
         * @param to
         *            состояние в которое должен перейти автомат
         * 
         * @return true - если переход добавлен. false - если для заданного текущего
         *         состояния и считанного символа уже определено состояние в которое
         *         нужно перейти.
         */

        protected void AddTransition(States from, char symbol, States to)
        {
            var transition = transitionTable[from];
            if (transition != null)
            {
                var transiton = transitionTable[from];
                if (transiton[symbol] != null)
                {
                    throw new Exception("already Exist");
                }
                else
                {
                    transiton.Add(symbol, to);
                }
            }
            else
            {
                transition = new Dictionary<char, States> {{symbol, to}};
                transitionTable.Add(from, transition);
            }
        }

        /**
         * Добавляет конечное состояние.
         * 
         * @param state
         *            состояние
         * 
         * @return true - если переход добавлен.
         */

        protected void AddFinalState(States state)
        {
            finalStates.Add(state);
        }

        /**
         * Устанавливает переход по умолчанию для текущего состояния. Если переход
         * по умолчанию уже был установлен, он заменяется.
         * 
         * @param from
         *            текущее состояние
         * @param to
         *            состоние в которое нужно перейти
         */

        protected void SetDefaultTransition(States from, States to)
        {
            defaultTransitions.Add(from, to);
        }

        /**
         * Устанавливает начальное состояние
         * 
         * @param state
         *            состояние
         */

        protected void SetInitState(States state)
        {
            initState = state;
        }
    }

}
