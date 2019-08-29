using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
namespace StudioX
{
    namespace Effects
    {
        [Serializable]
        [PostProcess(typeof(InsectEyeEffectRenderer), PostProcessEvent.BeforeStack, "Custom/InsectEye")]
        public sealed class InsectEyeEffect : PostProcessEffectSettings
        {
            [Range(0f, 1.0f), Tooltip("Blend intensity")]
            public FloatParameter blend = new FloatParameter { value = 0.25f };

            [Range(0f, 1.0f), Tooltip("Hexagon Mask Color")]
            public ColorParameter hexColor = new ColorParameter { value = new Color(1.0f, 1.0f, 1.0f, 1.0f) };
        }

        public sealed class InsectEyeEffectRenderer : PostProcessEffectRenderer<InsectEyeEffect>
        {
            public override void Render(PostProcessRenderContext context)
            {
                var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/InsectEye"));
                sheet.properties.SetFloat("_Blend", settings.blend);
                sheet.properties.SetColor("_HexColor", settings.hexColor);
                context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
            }
        }
    }
}

