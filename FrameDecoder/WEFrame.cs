using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameDecoder
{
    public class WEFrame
    {
        byte[] frameData;

        public byte[] FrameData
        {
            get { return frameData; }
            set { frameData = value; }
        }




        long deviceNum = -1;
        /// <summary>
        /// 设备ID 默认值-1
        /// </summary>
        public long DeviceNum
        {
            get { return deviceNum; }
            set { deviceNum = value; }
        }

        MainServeRequestResponse requestMark;
        /// <summary>
        /// 控制码第一位 发送命令，发出应答
        /// </summary>
        public MainServeRequestResponse RequestMark
        {
          get { return requestMark; }
            set { requestMark = value; }
        }


        MainServerResponseStatus responseStatus;
        /// <summary>
        /// 控制码第二位，是否出错
        /// </summary>
        public MainServerResponseStatus ResponseStatus
        {
            get { return responseStatus; }
            set { responseStatus = value; }
        }



        RequestResponseCode responseCode;
        /// <summary>
        /// 控制码中的命令、请求、应答 的功能代码
        /// </summary>
        public RequestResponseCode ResponseCode
        {
            get { return responseCode; }
            set { responseCode = value; }
        }


        long dataSize;
        /// <summary>
        /// 帧中显示的数据长度 用两位表示
        /// </summary>
        public long DataSize
        {
            get { return dataSize; }
            set { dataSize = value; }
        }

        bool isFrameCheckPass=false;
        /// <summary>
        /// 执行校验
        /// </summary>
        public bool IsFrameCheckPass
        {
            get {
                return isFrameCheckPass; 
            }
            set { isFrameCheckPass = value; }
        }




        /// <summary>
        /// 编码控制命令
        /// </summary>
        /// <returns></returns>
        public byte ControlcodeEncode() 
        {
            byte controlCode;
            //controlCode = BitConverter.GetBytes((int)requestMark + (int)responseStatus + (int)responseCode).First();
            controlCode = (byte)((byte)requestMark + (byte)responseStatus + (byte)responseCode);
            return controlCode;                            
        }


        /// <summary>
        /// 解码控制命令
        /// </summary>
        /// <param name="controlCode"></param>
        public void ControlcodeDecode(byte controlCode) 
        {
            byte[] toBitArray = { controlCode };
            BitArray tba = new BitArray(toBitArray);
            bool isSendResponse= tba.Get(7);
            bool isResponseError = tba.Get(6);
            byte responseCode = (byte)(controlCode <<3);
            responseCode = (byte)(responseCode >> 3);
            if (isSendResponse)
            {
                this.requestMark = MainServeRequestResponse.Response;
            }
            else
            {
                this.requestMark = MainServeRequestResponse.Request;
            }
            if (isResponseError)
            {
                this.responseStatus = MainServerResponseStatus.Error;
            }
            else
            {
                this.responseStatus = MainServerResponseStatus.NoError;
            }
            switch (responseCode)
            {
                case 0x00:
                    this.responseCode = RequestResponseCode.ReadVersion;
                    break;
                case 0x01:
                    this.responseCode = RequestResponseCode.SendVersion;
                    break;
                case 0x02:
                    this.responseCode = RequestResponseCode.ReadFileInfo;
                    break;
                case 0x03:
                    this.responseCode = RequestResponseCode.SendFileInfo;
                    break;
                case 0x04:
                    this.responseCode = RequestResponseCode.ReadFileByte;
                    break;
                case 0x05:
                    this.responseCode = RequestResponseCode.SendFileByte;
                    break;
                default:
                    break;
            }

        }





        /// <summary>
        /// 提取帧中的数据
        /// </summary>
        /// <param name="frameCode"></param>
        /// <returns></returns>
        public FrameDecodeResult DecodeFrameToData(byte[] frameCode)
        {
            byte[]deviceNumBCD_Code=new byte[6];
            Array.Copy(frameCode, 1, deviceNumBCD_Code, 0, 6);
            byte controlCodeSrc = frameCode[8];
            byte[]lsta=new byte[2]; //好像没什么用
            //byte[]data=new byte[frameCode.Length-13];

            this.deviceNum = readBCDDeviceNum(deviceNumBCD_Code);
            
            ControlcodeDecode(controlCodeSrc);
            //DataToFile(data);
            this.frameData = new byte[frameCode.Length - 13];
            Array.Copy(frameCode, 11, this.frameData, 0, this.frameData.Length);
            switch (this.responseCode)
            {
                case RequestResponseCode.ReadVersion:
                    break;
                case RequestResponseCode.SendVersion:
                    break;
                case RequestResponseCode.ReadFileInfo:
                    break;
                case RequestResponseCode.SendFileInfo:
                    
                    break;
                case RequestResponseCode.ReadFileByte:
                    break;
                case RequestResponseCode.SendFileByte:
                    break;
                default:
                    break;
            }
            return FrameDecodeResult.error_default;
        }

        /// <summary>
        /// 号码格式是BCD码  也就是 0x11 == 11        0x11 != 17
        /// </summary>
        /// <param name="BCDNum"></param>
        /// <returns></returns>
        public static long readBCDDeviceNum(byte[] BCDNum)
        {
            long result = 0;
            for (int i = 0; i < BCDNum.Length; i++)
            {
                byte item = BCDNum[i];
                byte right = (byte)((byte)(item << 4) >> 4);
                byte left = (byte)(item >> 4);
                result += (long)((left * 10 + right) * (Math.Pow(10, 2 * (BCDNum.Length - 1 - i))));
            }
            return result;
        }


        /// <summary>
        /// 转换成需要的数据 、文件
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        bool DataToFile(byte[] data)
        {
            bool result = false;

            throw new NotImplementedException("还没写");
            return result;
        }

        /// <summary>
        /// 封装数据帧
        /// </summary>
        /// <param name="dataCode">需要发送的数据</param>
        /// <param name="controlCode">控制代码</param>
        /// <returns></returns>
        private byte[] EncodeDataToFrame(byte[] dataCode,byte controlCode)
        {
            byte [] dataLength =BitConverter.GetBytes(dataCode.Length / 256);  //应该是两位
            byte[] headArray = { 0x68, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x68 ,controlCode};
            byte csCode = 0; //校验码  校验和 //Ps usb不是已经校验过了吗？
            int sumCode = 3000;
            byte csTestCode = BitConverter.GetBytes(sumCode % 256).First() ;

            byte[] dataCodeLength = {BitConverter.GetBytes(dataCode.Length%256).First(),BitConverter.GetBytes(dataCode.Length/256).First()};

            byte []frameCode=new byte[headArray.Length+2+dataCode.Length+1+1];
            int copyIndex = 0;
            headArray.CopyTo(frameCode, copyIndex);
            copyIndex += headArray.Length;

            dataCodeLength.CopyTo(frameCode, copyIndex);
            copyIndex += dataCodeLength.Length;

            dataCode.CopyTo(frameCode, copyIndex);
            copyIndex += dataCode.Length;

            frameCode[frameCode.Length - 2] = csCode;
            frameCode[frameCode.Length-1] = 0x16;

            return frameCode;
            //return FrameDecodeResult.error_default;
        }


        /// <summary>
        /// 封装数据帧
        /// </summary>
        /// <param name="dataCode">需要发送的数据</param>
        /// <param name="controlCode">控制代码</param>
        /// <returns></returns>
        public byte[] EncodeDataToFrame(WEFrame frame)
        {
            byte[] dataLength = BitConverter.GetBytes(frame.FrameData.Length / 256);  //应该是两位
            byte[] headArray = { 0x68, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x68, frame.ControlcodeEncode()};
            if (frame.DeviceNum>0)
            {
                byte []idByte=BitConverter.GetBytes(frame.DeviceNum);
                idByte.CopyTo(headArray, 1);
            }
            byte csCode = 0; //校验码  校验和 //Ps usb不是已经校验过了吗？
            int sumCode = 3000;
            byte csTestCode = BitConverter.GetBytes(sumCode % 256).First();

            byte[] dataCodeLength = { BitConverter.GetBytes(frame.FrameData.Length % 256).First(), BitConverter.GetBytes(frame.FrameData.Length / 256).First() };

            byte[] frameCode = new byte[headArray.Length + 2 + frame.FrameData.Length + 1 + 1];
            int copyIndex = 0;
            headArray.CopyTo(frameCode, copyIndex);
            copyIndex += headArray.Length;

            dataCodeLength.CopyTo(frameCode, copyIndex);
            copyIndex += dataCodeLength.Length;

            frame.FrameData.CopyTo(frameCode, copyIndex);
            copyIndex += frame.FrameData.Length;

            csCode = CalculateCsCode(frameCode, 0, frameCode.Length - 3);
            frameCode[frameCode.Length - 2] = csCode;
            frameCode[frameCode.Length - 1] = 0x16;

            return frameCode;
            //return FrameDecodeResult.error_default;
        }

        private byte CalculateCsCode(byte[] frameCode, int startIndex, int endIndex)
        {
            byte csCode = 0x00;
            while (startIndex<=endIndex)
            {
                csCode += frameCode[startIndex];
                startIndex++;
            }
            return csCode;
        }

        /// <summary>
        /// FrameData定义解析方式
        /// </summary>
        public void FrameDataDecode() 
        {
            if (this.FrameData!=null&&this.FrameData.Length>0)
            {

                //连接开始  
                //请求设备信息 //ID //Version //是否有新文件              //接收错误


                //请求文件信息    //文件名  //文件校验码？                 //接收错误
                //请求文件      //是否最后   //文件主体                 //接收错误
            }
        }


    }


    //byte MainServerSendRequest = 0x00;//0000 0000
    //byte MainServerSendResponse = 0x80;//1000 0000        
    //byte MainServerResponseNoError = 0x00;//0000 0000
    //byte mainServerResponseError = 0x40;//0100 0000
    //byte ResponseCode_Read = 0x01;//00001
    //byte ResponseCode_Write = 0x04;//00100
    //byte ResponseCode_WriteAdress = 0x0A;//01010 这 写地址 是毛线命令啊？
    //byte ResponseCode_ChangePWD = 0x0F;//01111
    /// <summary>
    /// 控制码第一位 发送命令，发出应答
    /// </summary>
    public enum MainServeRequestResponse
	{
        Request=0x00,
        Response=0x80
	}
    /// <summary>
    /// 控制码第二位，是否出错
    /// </summary>
    public enum MainServerResponseStatus
	{
        NoError=0x00,
        Error=0x40
	}

    /// <summary>
    /// 控制码中的命令、请求、应答 的功能代码
    /// </summary>
    public enum RequestResponseCode
	{
        ReadVersion=0x00,//请求Version
        SendVersion=0x01,//返回Version
        ReadFileInfo=0x02,//请求文件名
        SendFileInfo=0x03,//返回文件名
        ReadFileByte=0x04,//请求文件
        SendFileByte=0x05 ,//发送文件
        SetTime=0x06,
        //SendTime=0x07, //这里也许可以搞成心跳包  不需要回复
        SetSSID_PWD=0x08,//设置Wifi SSID 和PassWord



        ReadHeartBeat=0x0A,
        SendHeartBeat=0x0B,
	}
    public enum FrameCode
    {
        IDHeadEnd=0x68,
        IDFrameEnd=0x16
    }
    /// <summary>
    /// 帧解析结果
    /// </summary>
    public enum FrameDecodeResult
	{
        succuss,error_default,error_,error_Tolong
	}
}
