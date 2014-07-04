﻿using System;
using System.Linq;
using System.Xml.Linq;
using Shouldly;

namespace OpenRealEstate.Services
{
    public static class XElementExtensions
    {
        public static string Value(this XElement xElement,
            string elementName,
            string attributeName = null,
            string attributeValue = null)
        {
            var value = ValueOrDefault(xElement, elementName, attributeName, attributeValue);

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var errorMessage = string.Format("Expected the {0} '{1}' but failed to find it in the element '{2}' or that element exists, but with no data.",
                string.IsNullOrWhiteSpace(attributeName) ||
                string.IsNullOrWhiteSpace(attributeValue)
                    ? "element"
                    : "attribute",
                string.IsNullOrWhiteSpace(attributeName) ||
                string.IsNullOrWhiteSpace(attributeValue)
                    ? elementName
                    : attributeName,
                xElement.Name);
            throw new Exception(errorMessage);
        }

        public static string ValueOrDefault(this XElement xElement,
            string elementName,
            string attributeName = null,
            string attributeValue = null)
        {
            if (xElement == null)
            {
                throw new ArgumentNullException();
            }

            elementName.ShouldNotBeNullOrEmpty();

            var childElement = xElement.Element(elementName);
            if (childElement == null)
            {
                return null;
            }

            // We are either after the value of an attribute OR
            // the element value given an matching attribute AND an attribute value.
            if (!string.IsNullOrEmpty(attributeName))
            {
                var attribute = childElement.Attribute(attributeName);
                if (attribute == null)
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(attributeValue))
                {
                    return attribute.Value;
                }

                if (attribute.Value != attributeValue)
                {
                    return null;
                }
            }

            return childElement.Value.Trim();
        }

        public static string AttributeValue(this XElement xElement, string attributeName)
        {
            var value = AttributeValueOrDefault(xElement, attributeName);

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var errorMessage = string.Format("Expected the attribute '{0}' but failed to find it in the element '{1}'.",
                attributeName,
                xElement.Name);
            throw new Exception(errorMessage);
        }

        public static string AttributeValueOrDefault(this XElement xElement, string attributeName)
        {
            if (xElement == null)
            {
               throw new ArgumentNullException();
            }

            if (string.IsNullOrWhiteSpace(attributeName))
            {
                throw new ArgumentNullException("attributeName");
            }

            var attribute = xElement.Attribute(attributeName);
            return attribute == null
                ? null 
                : attribute.Value;
        }

        public static bool AttributeBoolValueOrDefault(this XElement xElement, string attributeName)
        {
            if (xElement == null)
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrWhiteSpace(attributeName))
            {
                throw new ArgumentNullException();
            }

            var attribute = xElement.Attribute(attributeName);
            if (attribute == null)
            {
                // No attribute (for this element) found.
                throw new ArgumentNullException();
            }

            // Check to see if this value can be converted to a bool. Ie. 0/1/true/false.
            bool boolValue;
            return bool.TryParse(attribute.Value, out boolValue)
                ? boolValue 
                : attribute.Value.ParseYesNoToBool();
        }

        public static int IntValueOrDefault(this XElement xElement, string childElementName = null)
        {
            if (xElement == null)
            {
                return 0;
            }

            // If we don't provide a child element name, then use the current one.
            var childElement = string.IsNullOrEmpty(childElementName)
                ? xElement
                : xElement.Element(childElementName);
            if (childElement == null)
            {
                return 0;
            }

            var value = childElement.Value;
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            int number;
            if (value.Contains("."))
            {
                int.TryParse(value.Substring(0, value.IndexOf(".", StringComparison.Ordinal)), out number);
            }
            else
            {
                int.TryParse(value, out number);
            }

            return number;
        }

        public static decimal DecimalValueOrDefault(this XElement xElement, string elementName)
        {
            if (xElement == null)
            {
                return 0;
            }

            var childElement = xElement.Element(elementName);
            if (childElement == null)
            {
                return 0;
            }

            var value = childElement.Value;
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            decimal number;
            decimal.TryParse(value, out number);

            return number;
        }

        public static byte ByteValueOrDefault(this XElement xElement, string childElementName = null)
        {
            if (xElement == null)
            {
                return 0;
            }

            // If we don't provide a child element name, then use the current one.
            var childElement = string.IsNullOrEmpty(childElementName)
                ? xElement
                : xElement.Element(childElementName);
            if (childElement == null)
            {
                return 0;
            }

            var value = childElement.Value;
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            byte number;
            if (value.Contains("."))
            {
                byte.TryParse(value.Substring(0, value.IndexOf(".", StringComparison.Ordinal)), out number);
            }
            else
            {
                byte.TryParse(value, out number);
            }

            return number;
        }

        public static bool BoolValueOrDefault(this XElement xElement, string childElementName = null)
        {
            if (xElement == null)
            {
                throw new ArgumentNullException("xElement");
            }

            // If we don't provide a child element name, then use the current one.
            var childElement = string.IsNullOrEmpty(childElementName)
                ? xElement
                : xElement.Element(childElementName);
            if (childElement == null)
            {
                // We've asked for a child element, but it doesn't exist.
                // Child elements are optional - which is why we're not throwning an exception.
                //return false;
                throw new Exception(
                    string.Format("The element '{0}' was not found. Please provide a valid element or Child Element.", childElementName));
            }

            var value = childElement.Value;
            if (string.IsNullOrEmpty(value))
            {
                // Element exists but it has no value.
                return false;
            }

            // Checking for 0/1/YES/NO
            bool boolValue;
            return bool.TryParse(value, out boolValue)
                ? boolValue
                : value.ParseYesNoToBool();
        }

        public static XElement StripNameSpaces(this XElement root)
        {
            var xElement = new XElement(
                root.Name.LocalName,
                root.HasElements ?
                    root.Elements().Select(StripNameSpaces) :
                    (object)root.Value
            );

            xElement.ReplaceAttributes(root.Attributes()
                .Where(attr => (!attr.IsNamespaceDeclaration)));

            return xElement;
        }
    }
}