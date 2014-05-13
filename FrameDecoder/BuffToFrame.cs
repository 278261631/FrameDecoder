using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameDecoder
{
    public class BuffToFrame
    {
        WEFrame frame=null;

        byte[] lastBuff;


        long nextLenth = 0;
        long nextStartIndex = 0;
        /// <summary>
        /// 
        /// </summary>
        public virtual void ResolveSessionBuffer(byte[] buff)
        {



            if (frame==null)
            {
                for (int i = 0; i < buff.Length-7; i++)
			    {
                    if (buff[i] == 0x68 && buff[i+7] == 0x68) //这里有头信息不全的情况需要处理
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
                        frame.FrameData = new byte[frame.DataSize];

                        Array.Copy(buff, i + 11, frame.FrameData, nextStartIndex, nextLenth);
                        //frame.FrameData=  
                        //decodeData  
	                  }
			      }
            }
            else
            {
                

            }

        }


        /// <summary>
        /// 这里用来测试吧，假定了frame大小小于1024 同时忽略包同时出现两个frame的情况 收发部分用问答模式 错误接受需要重发
        /// </summary>
        /// <param name="buff"></param>
        public virtual WEFrame ResolveSessionBufferSimple(byte[] buff)
        {

            for (int i = 0; i < buff.Length - 7; i++)
            {
                if (buff[i] == 0x68 && buff[i + 7] == 0x68) //这里有头信息不全的情况需要处理
                {
                    frame = new WEFrame();
                    byte[] idbuff = new byte[6];
                    Array.Copy(buff, 1, idbuff, 0, 6);
                    //frame.DeviceNum = BitConverter.ToInt64(buff,0);
                    frame.DeviceNum = WEFrame.readBCDDeviceNum(idbuff);
                    frame.ControlcodeDecode(buff[8]);
                    byte[] dataSizeBuff = new byte[2];
                    Array.Copy(buff, 9, dataSizeBuff, 0, 2);
                    frame.DataSize = BitConverter.ToInt64(dataSizeBuff, 0);
                    frame.FrameData = new byte[frame.DataSize];

                    Array.Copy(buff, i + 11, frame.FrameData, 0, frame.DataSize);
                    //frame.FrameData=  
                    //decodeData  

                    break;
                }
            }

            return frame;

        }
    }
}
