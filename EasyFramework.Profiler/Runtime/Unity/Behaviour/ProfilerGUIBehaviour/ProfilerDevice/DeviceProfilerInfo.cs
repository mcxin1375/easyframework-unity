using System;
using UnityEngine;

namespace EasyFramework.Profiler
{
    [Serializable]
    public class DeviceProfilerInfo
    {
        public string deviceUniqueIdentifier;   // 唯一设备标识符
        public DeviceType deviceType;           // 设备类型
        public string processorType;            // CPU型号
        public int processorFrequency;          // CPU频率
        public string graphicsDeviceName;       // GPU
        public long systemMemorySize;           // 内存 MG
        public long graphicsMemorySize;         // 显存 MB
        public int processorCount;              // 处理器个数
        public float startBatteryLevel;         // 初始电量
        public float batteryLevel;              // 当前电量
        public long monitorDurationSeconds;     // 监测时长 

        public string operatingSystem;          // 操作系统
        public string graphicsDeviceVersion;    // GPU版本
        public int graphicsShaderLevel;         // Shader级别
    }
}
