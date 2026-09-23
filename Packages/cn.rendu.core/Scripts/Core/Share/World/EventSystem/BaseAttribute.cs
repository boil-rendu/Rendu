using System;

namespace RD.Core
{
	[AttributeUsage(AttributeTargets.Class)]
	[EnableClass]
	public class BaseAttribute: EnableClassAttribute
	{
	}
}