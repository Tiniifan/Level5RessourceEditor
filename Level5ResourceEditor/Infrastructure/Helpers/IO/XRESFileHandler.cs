using System;
using System.Collections.Generic;
using System.Linq;
using StudioElevenLib.Level5.Resource;
using StudioElevenLib.Level5.Resource.XRES;
using StudioElevenLib.Level5.Resource.Types;
using StudioElevenLib.Level5.Resource.Types.Scene3D;
using Level5ResourceEditor.Services;

namespace Level5ResourceEditor.Infrastructure.Helpers.IO
{
    public class XRESFileHandler
    {
        public void SaveScene3D(Dictionary<RESType, List<RESElement>> items, string filePath, string magic)
        {
            // Filter and prepare items for XRES Scene3D
            var filteredItems = new Dictionary<RESType, List<RESElement>>();

            // Process materials from XRESSupport.TypeOrder
            foreach (var materialType in XRESSupport.TypeOrder)
            {
                // Only process materials for Scene3D
                if (IsScene3DMaterial(materialType))
                {
                    if (items.ContainsKey(materialType))
                    {
                        // Special handling for TextureData - only keep XRESTextureData
                        if (materialType == RESType.TextureData)
                        {
                            var textureData = items[materialType]
                                .Where(e => e.GetType() == typeof(XRESTextureData))
                                .ToList();

                            filteredItems[materialType] = textureData;
                        }
                        else
                        {
                            filteredItems[materialType] = new List<RESElement>(items[materialType]);
                        }
                    }
                    else
                    {
                        // Add empty list if key doesn't exist
                        filteredItems[materialType] = new List<RESElement>();
                    }
                }
            }

            // Process nodes for Scene3D
            var nodeTypes = new[] { RESType.MeshName, RESType.Bone, RESType.AnimationMTN2,
                RESType.AnimationIMN2, RESType.AnimationMTM2, RESType.Shading,
                RESType.Properties, RESType.MTNINF, RESType.IMMINF, RESType.MTMINF, RESType.Textproj };

            foreach (var nodeType in nodeTypes)
            {
                if (items.ContainsKey(nodeType))
                {
                    filteredItems[nodeType] = new List<RESElement>(items[nodeType]);
                }
                else
                {
                    // Add empty list if key doesn't exist
                    filteredItems[nodeType] = new List<RESElement>();
                }
            }

            // Create XRES and save
            var xres = new XRES { Items = filteredItems };
            xres.Save(magic, filePath);
        }

        public void SaveScene2D(Dictionary<RESType, List<RESElement>> items, string filePath, string magic)
        {
            throw new NotImplementedException(
                TranslationService.Instance.GetTranslation("Globals.Errors", "scene2DNotImplemented"));
        }

        private bool IsScene3DMaterial(RESType type)
        {
            return type == RESType.MaterialTypeUnk1 ||
                   type == RESType.Material1 ||
                   type == RESType.Material2 ||
                   type == RESType.TextureData ||
                   type == RESType.MaterialTypeUnk2 ||
                   type == RESType.MaterialData;
        }
    }
}