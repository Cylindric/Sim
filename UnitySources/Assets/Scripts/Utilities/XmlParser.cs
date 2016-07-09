using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public static class XmlParser
    {
        public static string ParseAttributeString(XmlNode node, string attribute, string defaultValue = "")
        {
            if (node == null) return defaultValue;
            if (node.Attributes == null) return defaultValue;

            var value = node.Attributes[attribute];
            return value == null ? defaultValue : value.InnerText;
        }

        public static string ParseString(XmlNode node, string xpath, string defaultValue = "")
        {
            if (node == null) return defaultValue;

            var value = node.SelectSingleNode(xpath);
            return value == null ? defaultValue : value.InnerText;
        }

        public static bool ParseBool(XmlNode node, string xpath, bool defaultValue = false)
        {
            if (node == null) return defaultValue;

            var value = node.SelectSingleNode(xpath);
            if (value == null) return defaultValue;

            var text = value.InnerText.ToLower();
            return (text == "1" || text == "true" || text == "on" || text == "yes");
        }

        public static int ParseInt(XmlNode node, string xpath, int defaultValue = 0)
        {
            if (node == null) return defaultValue;

            var value = node.SelectSingleNode(xpath);
            if (value == null) return defaultValue;

            int outVal;
            if (int.TryParse(value.InnerText, out outVal))
            {
                return outVal;
            }

            return defaultValue;
        }

        public static float ParseFloat(XmlNode node, string xpath, float defaultValue = 0f)
        {
            if (node == null) return defaultValue;

            var value = node.SelectSingleNode(xpath);
            if (value == null) return defaultValue;

            float outVal;
            if (float.TryParse(value.InnerText, out outVal))
            {
                return outVal;
            }

            return defaultValue;
        }

        public static Vector2 ParseVector2(XmlNode node, string xpath)
        {
            if (node == null) return Vector2.zero;

            var value = node.SelectSingleNode(xpath);
            if (value == null)
            {
                return Vector2.zero;
            }

            var parts = value.InnerText.Split(',');
            if (parts.Length < 2)
            {
                return Vector2.zero;
            }

            return new Vector2(
                int.Parse(parts[0]),
                int.Parse(parts[1]));
        }
    }
}