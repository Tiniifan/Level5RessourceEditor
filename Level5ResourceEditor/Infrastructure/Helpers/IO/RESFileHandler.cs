using System;
using System.Collections.Generic;
using System.Linq;
using StudioElevenLib.Level5.Resource;
using StudioElevenLib.Level5.Resource.RES;
using StudioElevenLib.Level5.Resource.XRES;
using StudioElevenLib.Level5.Resource.Types;
using StudioElevenLib.Level5.Resource.Types.Scene3D;

namespace Level5ResourceEditor.Infrastructure.Helpers.IO
{
    public class RESFileHandler
    {
        public void SaveScene3D(Dictionary<RESType, List<RESElement>> items, string filePath, string magic)
        {
            // Filter only materials and nodes for Scene3D
            var filteredItems = new Dictionary<RESType, List<RESElement>>();

            // Add materials
            foreach (var materialType in RESSupport.Materials)
            {
                if (items.ContainsKey(materialType))
                {
                    // Special handling for TextureData - only keep RESTextureData
                    if (materialType == RESType.TextureData)
                    {
                        var textureData = items[materialType]
                            .Where(e => e.GetType() == typeof(RESTextureData))
                            .ToList();

                        if (textureData.Count > 0)
                        {
                            filteredItems[materialType] = textureData;
                        }
                    }
                    else
                    {
                        filteredItems[materialType] = new List<RESElement>(items[materialType]);
                    }
                }
            }

            // Add nodes
            foreach (var nodeType in RESSupport.Nodes)
            {
                if (items.ContainsKey(nodeType))
                {
                    filteredItems[nodeType] = new List<RESElement>(items[nodeType]);
                }
            }

            // Create RES and save
            var res = new RES { Items = filteredItems };
            res.Save(magic, filePath);
        }

        public void SaveScene2D(Dictionary<RESType, List<RESElement>> items, string filePath, string magic)
        {
            throw new NotImplementedException("Scene2D save is not yet implemented");
        }
    }
}