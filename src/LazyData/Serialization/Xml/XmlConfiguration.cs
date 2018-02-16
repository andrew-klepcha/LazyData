﻿using System.Xml.Linq;

namespace LazyData.Serialization.Xml
{
    public class XmlConfiguration : SerializationConfiguration<XElement, XElement>
    {
        public static XmlConfiguration Default => new XmlConfiguration
        {
            TypeHandlers = new ITypeHandler<XElement, XElement>[0]
        };
    }
}