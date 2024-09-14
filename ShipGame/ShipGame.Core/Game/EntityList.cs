#region File Description
//-----------------------------------------------------------------------------
// EntityList.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
#endregion

namespace ShipGame
{
    public struct Entity
    {
        public String name;            // entity name
        public Matrix transform;       // entity transform matrix

        public Entity()
        {
            name = string.Empty;
            transform = Matrix.Identity;
        }
        /// <summary>
        /// Create a new entity with given name and transform matrix
        /// </summary>
        public Entity(String entityName, Matrix entityTransform)
        {
            name = entityName;
            transform = entityTransform;
        }
    }

    public class EntityList
    {
        // entities list
        public List<Entity> entities = new List<Entity>();

        // last random number generated (to prevent repetition)
        int lastRandom = -1;

        public EntityList () {}

        /// <summary>
        /// Get the entity transform matrix
        /// </summary>
        public Matrix GetTransform(String name)
        {
            foreach (Entity e in entities)
            {
                if (e.name == name)
                {
                    return e.transform;
                }
            }

            return Matrix.Identity;
        }

        /// <summary>
        /// Get a random transform matrix from the list preventing repetiton
        /// </summary>
        public Matrix GetTransformRandom(Random random)
        {
            // if no itens return indentity
            if (entities.Count == 0)
                return Matrix.Identity;

            // if only one item available return it
            if (entities.Count == 1)
                return entities[0].transform;

            // pick a random item different from the last one
            int rnd;
            do
            {
                rnd = random.Next(entities.Count);
            } while (rnd == lastRandom);

            // set new last random number
            lastRandom = rnd;

            // return transform for random pick
            return entities[rnd].transform;
        }

        /// <summary>
        /// Get the list of entities
        /// </summary>
        public List<Entity> Entities
        {
            get { return entities; }
        }

        /// <summary>
        /// Save the list to a xml file
        /// </summary>
        public bool Save(String filename)
        {
            // open stream
            Stream stream;
            stream = File.Create(filename);
            if (stream == null)
                return false;

            XDocument document = new XDocument ();
            document.Add ("EntityList", new XElement ("entities", () => {
                List<XElement> contents = new List<XElement>();
                foreach (Entity e in entities) {
                    XElement transform = new XElement("transform");
                    transform.Add ("M11", e.transform.M11);
                    transform.Add ("M12", e.transform.M12);
                    transform.Add ("M13", e.transform.M13);
                    transform.Add ("M14", e.transform.M14);
                    transform.Add ("M21", e.transform.M21);
                    transform.Add ("M22", e.transform.M22);
                    transform.Add ("M23", e.transform.M23);
                    transform.Add ("M24", e.transform.M24);
                    transform.Add ("M31", e.transform.M31);
                    transform.Add ("M32", e.transform.M32);
                    transform.Add ("M33", e.transform.M33);
                    transform.Add ("M34", e.transform.M34);
                    transform.Add ("M41", e.transform.M41);
                    transform.Add ("M42", e.transform.M42);
                    transform.Add ("M43", e.transform.M43);
                    transform.Add ("M44", e.transform.M44);
                    contents.Add (new XElement("Entity", new XElement ("name"), transform));
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
        /// Static function to load a entity list from a xml file
        /// </summary>
        public static EntityList Load(String filename)
        {
            // open file
            Stream stream;
            try
            {
                stream = TitleContainer.OpenStream(filename);
            }
            catch (FileNotFoundException e)
            {
                System.Console.WriteLine("EntityList load error:" + e.Message);
                stream = null;
            }
            if (stream == null)
                return null;

            EntityList entityList = new EntityList();
            XDocument document = XDocument.Load(stream);
            var entityElements = document.Descendants("Entity");
            IEnumerable<Entity> entities = entityElements.Select(element => new Entity
            {
                name = element.Element("name")?.Value ?? "unknown",
                transform = new Matrix (
                    float.Parse (element.Element("transform")?.Element ("M11")?.Value ?? "0"),
                    float.Parse (element.Element("transform")?.Element ("M12")?.Value ?? "0"),
                    float.Parse (element.Element("transform")?.Element ("M13")?.Value ?? "0"),
                    float.Parse (element.Element("transform")?.Element ("M14")?.Value ?? "0"),
                    float.Parse (element.Element("transform")?.Element ("M21")?.Value ?? "0"),
                    float.Parse (element.Element("transform")?.Element ("M22")?.Value ?? "0"),
                    float.Parse (element.Element("transform")?.Element ("M23")?.Value ?? "0"),
                    float.Parse (element.Element("transform")?.Element ("M24")?.Value ?? "0"),
                    float.Parse (element.Element("transform")?.Element ("M31")?.Value ?? "0"),
                    float.Parse (element.Element("transform")?.Element ("M32")?.Value ?? "0"),
                    float.Parse (element.Element("transform")?.Element ("M33")?.Value ?? "0"),
                    float.Parse (element.Element("transform")?.Element ("M34")?.Value ?? "0"),
                    float.Parse (element.Element("transform")?.Element ("M41")?.Value ?? "0"),
                    float.Parse (element.Element("transform")?.Element ("M42")?.Value ?? "0"),
                    float.Parse (element.Element("transform")?.Element ("M43")?.Value ?? "0"),
                    float.Parse (element.Element("transform")?.Element ("M44")?.Value ?? "0")
                ),
            });
            foreach (Entity entity in entities)
            {
                entityList.entities.Add(entity);
            }

            // close
            stream.Close();
            stream = null;

            return entityList;
        }
    }
}
