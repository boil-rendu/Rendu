using System;
using RD.Core;

namespace RD
{
    public class AIHandlerAttribute: BaseAttribute
    {
    }
    
    [AIHandler]
    public abstract class AAIHandler: HandlerObject
    {
        // 检查是否满足条件
        public abstract int Check(AIComponent aiComponent, AIConfig aiConfig);

        // 协程编写必须可以取消
        public abstract Promise Execute(AIComponent aiComponent, AIConfig aiConfig);
    }
}