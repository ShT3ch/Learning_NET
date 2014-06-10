using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proto_Parser
{
    public class Token
    {
        private Object value;
        private String text;
        private String type;

        /**
	 * @param type
	 *            строка, число, ключевое слово и т.п. Определяется Reader-ом,
	 *            создающим токен.
	 * @param text
	 *            подстрока исходного текста, которая превратилась в этот токен.
	 * @param value
	 *            для чисел — int или double, для строк — строка и т.п.
	 */

        public Token(String type, String text, Object value)
        {
            this.type = type;
            this.value = value;
            this.text = text;
        }

        public Token(String type, String text) : this(type, text, text)
        {}

        public Object Value
        {
            get{return value;}
        }

        public String TokenType
        {
            get { return type; }
        }

        public String Text
        {
            get { return text; }
        }

        public String toString()
        {
            return type + "[" + text + "]";
        }

        public bool equals(Object obj)
        {
            if (obj.GetType().IsInstanceOfType(this))
            {

                var other = (Token) obj;
                return type.Equals(other.type) && value.Equals(other.value)
                       && text.Equals(other.text);
            }
            else
            {
                return false;
            }
        }
    }
}
