using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CachingDNS
{
    internal class QueriesBlock
    {
        private byte[] block;
        private char[] name;
        private byte[] type;
        private byte[] klass;

        private int indexOfReaden;

        public string getNameOfQuery()
        {
            return new string(name);
        }

        public override int GetHashCode()
        {
            int hash = 0;
            hash += type.Sum(elem => elem.GetHashCode());
            hash += klass.Sum(elem => elem.GetHashCode());
            hash += name.Sum(elem => elem.GetHashCode());
            return hash;
        }

        public override bool Equals(object obj)
        {
            //            bool hash = true;

            if (type.Where((t, i) => !((QueriesBlock)obj).type[i].Equals(this.type[i])).Any())
            {
                return false;
            }

            if (klass.Where((t, i) => !((QueriesBlock)obj).klass[i].Equals(this.klass[i])).Any())
            {
                return false;
            }

            if (name.Where((t, i) => !((QueriesBlock)obj).name[i].Equals(this.name[i])).Any())
            {
                return false;
            }

            return true;
        }

        public QueriesBlock(byte[] input)
        {
            block = input;

            indexOfReaden = 0;

            var lengthOfName = input.Length - 4;

            name = new char[lengthOfName];
            Array.Copy(block, indexOfReaden, name, 0, lengthOfName);
            indexOfReaden += lengthOfName;
           // name = name.Reverse().ToArray();

            type = new byte[2];
            Array.Copy(block, indexOfReaden, type, 0, 2);
            indexOfReaden += 2;
            type = type.Reverse().ToArray();

            klass = new byte[2];
            Array.Copy(block, indexOfReaden, klass, 0, 2);
            indexOfReaden += 2;
            klass = klass.Reverse().ToArray();

        }
    }

    class AnswerBlock
    {
        private byte[] block;
        private char[] name;
        private byte[] type;
        private byte[] klass;
        private byte[] ttl;
        private byte[] dataLength;
        private byte[] data;
        private int indexOfReaden;
        public int TTL;

        public override int GetHashCode()
        {
            int hash = 0;
            hash += type.Sum(elem => elem.GetHashCode());
            hash += klass.Sum(elem => elem.GetHashCode());
            hash += data.Sum(elem => elem.GetHashCode());
            return hash;
        }

        public override bool Equals(object obj)
        {
//            bool hash = true;

            if (type.Where((t, i) => !((AnswerBlock)obj).type[i].Equals(this.type[i])).Any())
            {
                return false;
            }

            if (klass.Where((t, i) => !((AnswerBlock)obj).klass[i].Equals(this.klass[i])).Any())
            {
                return false;
            }

            if (data.Where((t, i) => !((AnswerBlock)obj).data[i].Equals(this.data[i])).Any())
            {
                return false;
            }

            return true;
        }

        public void setTtl(int newTTL)
        {
            TTL = newTTL;
        }

        public AnswerBlock(byte[] input)
        {
            block = input;

            indexOfReaden = 0;

            name = new char[2];
            Array.Copy(block, indexOfReaden, name, 0, 2);
            indexOfReaden += 2;
            name = name.Reverse().ToArray();

            type = new byte[2];
            Array.Copy(block, indexOfReaden, type, 0, 2);
            indexOfReaden += 2;
            type = type.Reverse().ToArray();

            klass = new byte[2];
            Array.Copy(block, indexOfReaden, klass, 0, 2);
            indexOfReaden += 2;
            klass = klass.Reverse().ToArray();

            ttl = new byte[4];
            Array.Copy(block, indexOfReaden, ttl, 0, 4);
            indexOfReaden += 4;
            ttl = ttl.Reverse().ToArray();

            dataLength = new byte[2];
            Array.Copy(block, indexOfReaden, dataLength, 0, 2);
            indexOfReaden += 2;
            dataLength = dataLength.Reverse().ToArray();
            var length = BitConverter.ToInt16(dataLength,0);

            data = new byte[length];
            Array.Copy(block, indexOfReaden, data, 0, length);
            indexOfReaden += length;

            TTL = BitConverter.ToInt32(ttl, 0);

        }

    }
    class DNSPacket
    {
        private byte[] packet;
        private byte[] identifier;
        private byte[] header;
        private byte[] numOfQuestions;
        private byte[] numOfAnswers;
        private byte[] numOfAuth;
        private byte[] numOfAdditional;
        private List<byte[]> listOfQueries;
        private List<byte[]> listOfAnswers;
        private List<byte[]> listOfAuthServers;
        private List<byte[]> listOfAdditional;
        private int indexOfReaden;

        public List<AnswerBlock> ParsedAnswers;
        public List<QueriesBlock> ParsedQueries;

        private int recievedTime;
        public int RecievedTime
        {
            get { return recievedTime; }
        }

        public void SetIndex(byte[] newIndex)
        {
            identifier = newIndex;
        }
        
        public void SetTTL(int newTTL)
        {
            foreach (var parsedAnswer in ParsedAnswers)
            {
                parsedAnswer.setTtl(newTTL);
            }
        }

        public byte[] makeAnswer(DNSPacket query)
        {
            this.setID(query.getID());
            return this.packet;
        }

        public void setID(Int16 id)
        {
            var bytes = BitConverter.GetBytes(id).Reverse().ToArray();
            for (int i = 0; i < 2; i++)
            {
                packet[i] = bytes[i];
            }
        }

        public Int16 getID()
        {
            return BitConverter.ToInt16(identifier, 0);
        }

        public int getMinTTL()
        {
            var min = ParsedAnswers[0].TTL;
            return ParsedAnswers.Select(parsedAnswer => parsedAnswer.TTL).Concat(new[] { min }).Min();
        }

        public byte[] getBytes()
        {
            return packet;
        }

        public string allQueries()
        {
            return ParsedQueries.Aggregate("", (current, queriesBlock) => current + (queriesBlock.getNameOfQuery() + " "));
        }

        public bool IsPacketAnswer()
        {
            return (header[0]>>7).Equals(1);
        }

        public bool IsPacketSucces()
        {
            var temp = header[0] & 0xf;
            return (temp).Equals(0) && this.ParsedAnswers.Count > 0;
        }

        public override int GetHashCode()
        {
            return ParsedQueries.Sum(elem => elem.GetHashCode());
        }

        public override bool Equals(object o)
        {
            if (o.GetType().IsInstanceOfType(this))
            {
                if (this.ParsedQueries.All(first => ((DNSPacket)o).ParsedQueries.Any(first.Equals)))
                    if (
                        ((DNSPacket) o).ParsedQueries.All(
                            first => this.ParsedQueries.Any(first.Equals)))
                    {
                        return true;
                    }
                return false;
            }
            else
            {
                return false;
            }
        }

        public DNSPacket(byte[] packet)
        {
            this.packet = packet;

            indexOfReaden = 0;

            identifier = new byte[2];
            Array.Copy(packet, indexOfReaden, identifier, 0, 2);
            indexOfReaden += 2;
            identifier = identifier.Reverse().ToArray();

            header = new byte[2];
            Array.Copy(packet, indexOfReaden, header, 0, 2);
            indexOfReaden += 2;
            header = header.Reverse().ToArray();

            numOfQuestions = new byte[2];
            Array.Copy(packet, indexOfReaden, numOfQuestions, 0, 2);
            indexOfReaden += 2;
            numOfQuestions = numOfQuestions.Reverse().ToArray();

            numOfAnswers = new byte[2];
            Array.Copy(packet, indexOfReaden, numOfAnswers, 0, 2);
            indexOfReaden += 2;
            numOfAnswers = numOfAnswers.Reverse().ToArray();

            numOfAuth = new byte[2];
            Array.Copy(packet, indexOfReaden, numOfAuth, 0, 2);
            indexOfReaden += 2;
            numOfAuth = numOfAuth.Reverse().ToArray();

            numOfAdditional = new byte[2];
            Array.Copy(packet, indexOfReaden, numOfAdditional, 0, 2);
            indexOfReaden += 2;
            numOfAdditional = numOfAdditional.Reverse().ToArray();

            listOfAdditional = new List<byte[]>();
            listOfAnswers = new List<byte[]>();
            listOfAuthServers = new List<byte[]>();
            listOfQueries = new List<byte[]>();
            ParsedAnswers = new List<AnswerBlock>();
            ParsedQueries =  new List<QueriesBlock>();

            for (var i = BitConverter.ToInt16(numOfQuestions,0); i > 0; i--)
            {
                var lengthOfName = 0;
                for (var j = 1; j < indexOfReaden + packet.Length; j++)
                {
                    if (packet[j + indexOfReaden] == 0)
                    {
                        lengthOfName = j;
                        break;
                    }
                }
                int length = 4+lengthOfName+1;

                var temp = new byte[length];
                
                Array.Copy(packet, indexOfReaden, temp, 0, length);
                indexOfReaden += length;
                listOfQueries.Add(temp);
            }

            foreach (var block in listOfQueries)
            {
                ParsedQueries.Add(new QueriesBlock(block));
            }

            for (var i = BitConverter.ToInt16(numOfAnswers,0); i > 0; i--)
            {
                var datalength = new byte[2];
                Array.Copy(packet, indexOfReaden + 10, datalength, 0, 2);
                datalength = datalength.Reverse().ToArray();

                var length = 12 + BitConverter.ToInt16(datalength,0);
                var temp = new byte[length];
                Array.Copy(packet, indexOfReaden, temp, 0, length);
                indexOfReaden += length;
                listOfAnswers.Add(temp);
            }

            foreach (var block in listOfAnswers)
            {
                ParsedAnswers.Add(new AnswerBlock(block));
            }

            for (var i = BitConverter.ToInt16(numOfAuth,0); i > 0; i--)
            {
                var temp = new byte[18];
                Array.Copy(packet, indexOfReaden, temp, 0, 18);
                indexOfReaden += 18;
                listOfAuthServers.Add(temp);
            }

            for (var i = BitConverter.ToInt16(numOfAdditional,0); i > 0; i--)
            {
                var temp = new byte[16];
                Array.Copy(packet, indexOfReaden, temp, 0, 16);
                indexOfReaden += 16;
                listOfAdditional.Add(temp);
            }
        }
    }
}
