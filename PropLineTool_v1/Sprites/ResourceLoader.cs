using ICities;

using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Globalization;

using UnityEngine;

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

//This class (ResourceLoader) courtesy of SamsamTS:
//https://github.com/SamsamTS/CS-FineRoadTool/blob/master/FineRoadTool/ResourceLoader.cs

namespace PropLineTool.Sprites {
    //Special Thanks to SamsamTS! (:
    internal static class ResourceLoader {
        /// <summary>
        /// Create a new atlas.
        /// </summary>
        public static UITextureAtlas CreateTextureAtlas(string atlasName, string[] spriteNames, string assemblyPath) {
            int maxSize = 1024;
            Texture2D texture2D = new Texture2D(maxSize, maxSize, TextureFormat.ARGB32, false);
            Texture2D[] textures = new Texture2D[spriteNames.Length];
            Rect[] regions = new Rect[spriteNames.Length];

            for (int i = 0; i < spriteNames.Length; i++)
                textures[i] = loadTextureFromAssembly(assemblyPath + spriteNames[i] + ".png");

            regions = texture2D.PackTextures(textures, 2, maxSize);

            UITextureAtlas textureAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            Material material = UnityEngine.Object.Instantiate<Material>(UIView.GetAView().defaultAtlas.material);
            material.mainTexture = texture2D;
            textureAtlas.material = material;
            textureAtlas.name = atlasName;

            for (int i = 0; i < spriteNames.Length; i++) {
                UITextureAtlas.SpriteInfo item = new UITextureAtlas.SpriteInfo {
                    name = spriteNames[i],
                    texture = textures[i],
                    region = regions[i],
                };

                textureAtlas.AddSprite(item);
            }

            return textureAtlas;
        }

        /// <summary>
        /// Add existing textures to an atlas.
        /// </summary>
        public static void AddTexturesInAtlas(UITextureAtlas atlas, Texture2D[] newTextures, bool locked = false) {
            Texture2D[] textures = new Texture2D[atlas.count + newTextures.Length];

            for (int i = 0; i < atlas.count; i++) {
                Texture2D texture2D = atlas.sprites[i].texture;

                if (locked) {
                    // Locked textures workaround
                    RenderTexture renderTexture = RenderTexture.GetTemporary(texture2D.width, texture2D.height, 0);
                    Graphics.Blit(texture2D, renderTexture);

                    RenderTexture active = RenderTexture.active;
                    texture2D = new Texture2D(renderTexture.width, renderTexture.height);
                    RenderTexture.active = renderTexture;
                    texture2D.ReadPixels(new Rect(0f, 0f, (float)renderTexture.width, (float)renderTexture.height), 0, 0);
                    texture2D.Apply();
                    RenderTexture.active = active;

                    RenderTexture.ReleaseTemporary(renderTexture);
                }

                textures[i] = texture2D;
                textures[i].name = atlas.sprites[i].name;
            }

            for (int i = 0; i < newTextures.Length; i++)
                textures[atlas.count + i] = newTextures[i];

            Rect[] regions = atlas.texture.PackTextures(textures, atlas.padding, 4096, false);

            atlas.sprites.Clear();

            for (int i = 0; i < textures.Length; i++) {
                UITextureAtlas.SpriteInfo spriteInfo = atlas[textures[i].name];
                atlas.sprites.Add(new UITextureAtlas.SpriteInfo {
                    texture = textures[i],
                    name = textures[i].name,
                    border = (spriteInfo != null) ? spriteInfo.border : new RectOffset(),
                    region = regions[i]
                });
            }

            atlas.RebuildIndexes();
        }

        /// <summary>
        /// Find an atlas that already exists.
        /// </summary>
        public static UITextureAtlas GetAtlas(string name) {
            UITextureAtlas[] atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
            for (int i = 0; i < atlases.Length; i++) {
                if (atlases[i].name == name)
                    return atlases[i];
            }

            return UIView.GetAView().defaultAtlas;
        }

        /// <summary>
        /// Used to load embedded images from assembly.
        /// </summary>
        private static Texture2D loadTextureFromAssembly(string path) {
            Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);

            byte[] array = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(array, 0, array.Length);

            Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            texture2D.LoadImage(array);

            return texture2D;
        }

        //=====================
        //==== Extra Stuff
        //=====================

        /// <summary>
        /// Used in conjuction with AddTexturesInAtlas to copy existing sprites into a different atlas.
        /// </summary>
        public static Texture2D[] FindSprites(string[] spriteNames, string atlasName) {
            UITextureAtlas _atlas = GetAtlas(atlasName);

            List<Texture2D> _textures = new List<Texture2D>(spriteNames.Length);

            Texture2D _textureLocal = new Texture2D(8, 8);

            for (int i = 0; i < spriteNames.Length; i++) {
                _textureLocal = _atlas[spriteNames[i]].texture;

                if (_textureLocal != null) {
                    _textures.Add(_textureLocal);
                }
            }

            return _textures.ToArray();
        }
    }
}