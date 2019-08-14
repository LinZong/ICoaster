using System.Collections.Generic;
using System.Linq;

namespace SNotiSSL.Model
{
    public class SNotiCommandType
    {

        public static readonly SNotiCommandType Login_Req = new SNotiCommandType("login_req", "");
        public static readonly SNotiCommandType Login_Res = new SNotiCommandType("login_res", "");
        public static readonly SNotiCommandType Event_Push = new SNotiCommandType("event_push", "");
        public static readonly SNotiCommandType Event_ACK = new SNotiCommandType("event_ack", "");
        public static readonly SNotiCommandType Remote_Control_V2_Req = new SNotiCommandType("remote_control_v2_req", "");
        public static readonly SNotiCommandType Remote_Control_V2_Res = new SNotiCommandType("remote_control_v2_res","");
        public static readonly SNotiCommandType Ping = new SNotiCommandType("ping","{\"cmd\":\"ping\"}\n");
        public static readonly SNotiCommandType Pong = new SNotiCommandType("pong","{\"cmd\":\"pong\"}\n");
        public static readonly SNotiCommandType InvalidMsg = new SNotiCommandType("invalid_msg","");

        public static readonly Dictionary<string,SNotiCommandType> CommandsDic;
        public SNotiCommandType(string cmd, string order)
        {
            Cmd = cmd;
            Order = order;
        }
        static SNotiCommandType()
        {
            CommandsDic =  typeof(SNotiCommandType).GetFields()
                                    .Where(t => t.FieldType == typeof(SNotiCommandType))
                                    .Select(t => t.GetValue(null))
                                    .Cast<SNotiCommandType>()
                                    .ToDictionary(s => s.Cmd);
        }
        public string Cmd { get; private set; }
        public string Order { get; private set; }

        public override string ToString()
        {
            return this.Cmd;
        }

        public override bool Equals(object obj)
        {
            if(obj is SNotiCommandType)
            {
                return this.Cmd == ((SNotiCommandType)obj).Cmd;
            }
            return false;
        }

        public static SNotiCommandType Parse(string command, string order = "")
        {
            return new SNotiCommandType(command,order);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}