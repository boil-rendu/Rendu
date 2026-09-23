using RD;
using RD.Core;

namespace RD.Server
{
    public struct RobotCase_SecondCaseWait: IWaitType
    {
        public int Error { get; set; }

        public M2C_TestRobotCase2 M2CTestRobotCase2;
    }
}