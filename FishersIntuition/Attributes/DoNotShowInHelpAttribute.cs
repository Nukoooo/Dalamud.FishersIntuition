using System;

namespace FishersIntuition.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class DoNotShowInHelpAttribute : Attribute
{
}