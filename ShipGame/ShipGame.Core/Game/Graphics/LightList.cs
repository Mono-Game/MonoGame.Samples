#region File Description
//-----------------------------------------------------------------------------
// LightList.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
#endregion

namespace ShipGame
{
    public struct Light
    {
        public Vector3 position;        // position
        public float radius;       // radius
        public Vector3 color;      // color

        public Light()
        {
            position = Vector3.Zero;
            radius = 1.0f;
            color = Vector3.One;
        }
        /// <summary>
        /// Create a new list of lights
        /// </summary>
        public Light(Vector3 lightPosition, float lightRadius, Vector3 lightColor)
        {
            position = lightPosition;
            radius = lightRadius;
            color = lightColor;
        }

        /// <summary>
        /// Set light properties to given effect
        /// </summary>
        public void SetEffect(
                EffectParameter effectLightPosition,
                EffectParameter effectLightColor,
                Matrix worldInverse)
        {
            Vector4 positionRadius =
                new Vector4(Vector3.Transform(position, worldInverse), radius);
            if (effectLightPosition != null)
            {
                effectLightPosition.SetValue(positionRadius);
            }
            if (effectLightColor != null)
            {
                effectLightColor.SetValue(color);
            }
        }
    }

    public class LightList
    {
        // ambient light
        public Vector3 ambient = new Vector3(0.3f, 0.3f, 0.3f);

        // list of lights
        public List<Light> lights = new List<Light>();

        public LightList() {}

        /// <summary>
        /// Saves the light list to a xml file
        /// </summary>
        public bool Save(String filename)
        {
            // create stream
            Stream stream;
            stream = File.Create(filename);
            if (stream == null)
                return false;

            XDocument document = new XDocument ();
            XElement amb = new XElement("ambient",
                new XElement ("X", ambient.X),
                new XElement ("Y", ambient.Y),
                new XElement ("Z", ambient.Z));
            document.Add ("LightList", amb, new XElement ("lights", () => {
                List<XElement> contents = new List<XElement>();
                foreach (Light e in lights) {
                    XElement position = new XElement("position",
                        new XElement ("X", e.position.X),
                        new XElement ("Y", e.position.Y),
                        new XElement ("Z", e.position.Z));
                    XElement radius = new XElement ("radius", e.radius);
                    XElement color = new XElement("color",
                        new XElement ("X", e.color.X),
                        new XElement ("Y", e.color.Y),
                        new XElement ("Z", e.color.Z));
                    contents.Add (new XElement("Light", position, radius, color));
                }
                return contents;
            } )); 

            document.Save (stream);

            // close
            stream.Close();
            stream = null;

            return true;
        }

        /// <summary>
        /// Static method to load a light list from a file
        /// </summary>
        public static LightList Load(String filename)
        {
            // open file
            Stream stream;
            try
            {
                stream = TitleContainer.OpenStream(filename);
            }
            catch (FileNotFoundException e)
            {
                System.Console.WriteLine("LightList load error:" + e.Message);
                stream = null;
            }
            if (stream == null)
                return null;

            // load data
            LightList environmentLights = new LightList();
            XDocument document = XDocument.Load(stream);
            XElement ambient = document.Element ("LightList").Element("ambient");
            environmentLights.ambient = new Vector3(
                float.Parse(ambient.Element("X")?.Value ?? "0.3"),
                float.Parse(ambient.Element("Y")?.Value ?? "0.3"),
                float.Parse(ambient.Element("Z")?.Value ?? "0.3")
            );
            var lightElements = document.Descendants("Light");
            IEnumerable<Light> lights = lightElements.Select(lightElement => new Light
            {
                position = new Vector3(
                    float.Parse(lightElement.Element("position")?.Element("X")?.Value ?? "0"),
                    float.Parse(lightElement.Element("position")?.Element("Y")?.Value ?? "0"),
                    float.Parse(lightElement.Element("position")?.Element("Z")?.Value ?? "0")
                ),
                radius = float.Parse(lightElement.Element("radius")?.Value ?? "1"),
                color = new Vector3(
                    float.Parse(lightElement.Element("color")?.Element("X")?.Value ?? "0"),
                    float.Parse(lightElement.Element("color")?.Element("Y")?.Value ?? "0"),
                    float.Parse(lightElement.Element("color")?.Element("Z")?.Value ?? "0")
                )
            });
            foreach (Light light in lights)
            {
                environmentLights.lights.Add(light);
            }
            
            
            // close
            stream.Close();
            stream = null;

            return environmentLights;
        }
    }
}
