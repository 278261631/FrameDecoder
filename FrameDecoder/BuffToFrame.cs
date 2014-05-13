using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameDecoder
{
    public class BuffToFrame
    {
        WEFrame frame=null;

        
        /// <summary>
        /// 提取包时与包规则紧密相关，根据实际规则重定义  //应该放在Decoder里
        /// </summary>
        public virtual void ResolveSessionBuffer(byte[] buff)
        {
            if (frame==null)
            {
                if (buff[0]==0x68&&buff[7]==0x68)
                {
                    frame = new WEFrame();
                    byte[]idbuff=new byte[6];
                    Array.Copy(buff, 1, idbuff,0,6);
                    //frame.DeviceNum = BitConverter.ToInt64(buff,0);
                    frame.DeviceNum = WEFrame.readBCDDeviceNum(idbuff);
                    frame.ControlcodeDecode(buff[8]);
                    byte[]dataSizeBuff=new byte[2];
                    Array.Copy(buff,9,dataSizeBuff,0,2);
                    frame.DataSize = BitConverter.ToInt64(dataSizeBuff, 0);
                        
                    //decodeData

                }
            }
            else
            {
                

            }

        }
    }
}
