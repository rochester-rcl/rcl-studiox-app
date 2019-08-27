using System;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine;
namespace StudioX
{
    namespace Effects
    {
        [Serializable]
        [PostProcess(typeof(InsectEyeEffectRenderer), PostProcessEvent.BeforeStack, "Custom/InsectEye")]
        public sealed class InsectEyeEffect : PostProcessEffectSettings
        {
            [Range(0f, 25f), Tooltip("Distortion intensity")]
            public FloatParameter distortion = new FloatParameter { value = 1.0f };
        }

        public sealed class InsectEyeEffectRenderer : PostProcessEffectRenderer<InsectEyeEffect>
        {
            public override void Render(PostProcessRenderContext context)
            {
                var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/InsectEye"));
                sheet.properties.SetFloat("_Distortion", settings.distortion);
                context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
            }
        }
    }
}

