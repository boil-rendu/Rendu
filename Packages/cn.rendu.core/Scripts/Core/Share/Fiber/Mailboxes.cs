using System.Collections.Generic;

namespace RD.Core
{
    /// <summary>
    /// 邮件箱集合：按 <c>Parent.InstanceId</c> 索引一组 <see cref="Entity"/> 引用，提供 <c>Add</c>/<c>Remove</c>/<c>Get</c> 操作。
    /// 主要用于将 Unit 等实体的「消息/收件」子实体挂到父实体的 <c>Parent</c> 上，以父 <c>InstanceId</c> 为键聚合。
    /// </summary>
    public class Mailboxes
    {
        private readonly Dictionary<long, EntityRef<Entity>> mailboxes = new();

        /// <summary>添加一个邮件箱实体：键为 <c>mailBox.Parent.InstanceId</c>，值为 <c>mailBox</c> 自身。</summary>
        /// <param name="mailBox">要注册的邮件箱实体（必须已挂到某个 Parent）。</param>
        public void Add(Entity mailBox)
        {
            this.mailboxes.Add(mailBox.Parent.InstanceId, mailBox);
        }

        /// <summary>按 <paramref name="instanceId"/> 移除一个邮件箱。</summary>
        /// <param name="instanceId">即 <c>Entity.Parent.InstanceId</c>（添加时使用的键）。</param>
        public void Remove(long instanceId)
        {
            this.mailboxes.Remove(instanceId);
        }

        /// <summary>按 <paramref name="instanceId"/> 获取邮件箱实体，未找到返回 default(<see cref="EntityRef{Entity}"/>)。</summary>
        /// <param name="instanceId">即 <c>Entity.Parent.InstanceId</c>。</param>
        public EntityRef<Entity> Get(long instanceId)
        {
            this.mailboxes.TryGetValue(instanceId, out EntityRef<Entity> entity);
            return entity;
        }
    }
}
