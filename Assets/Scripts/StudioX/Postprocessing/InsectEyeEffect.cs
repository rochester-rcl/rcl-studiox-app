using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
namespace StudioX
{
    namespace Effects
    {
        [Serializable]
        [PostProcess(typeof(InsectEyeEffectRenderer), PostProcessEvent.BeforeStack, "Custom/InsectEye")]
        /// <summary> Post-processing effect to display a compound eye over the frame. </summary>
        public sealed class InsectEyeEffect : PostProcessEffectSettings
        {
            /// <summary>The amount to blend the effect with the rendered frame.</summary>
            [Range(0f, 1.0f), Tooltip("Blend intensity")]
            public FloatParameter blend = new FloatParameter { value = 0.25f };
            /// <summary>The color of the compound eye outline.</summary>
            [Range(0f, 1.0f), Tooltip("Hexagon Mask Color")]
            public ColorParameter hexColor = new ColorParameter { value = new Color(1.0f, 1.0f, 1.0f, 1.0f) };
        }
        /// <summary>
        /// The <see cref="UnityEngine.Rendering.PostProcessing.PostProcessEffectRenderer"/>
        /// for <see cref="InsectEyeEffect"/>.
        /// </summary>
        public sealed class InsectEyeEffectRenderer : PostProcessEffectRenderer<InsectEyeEffect>
        {
            /// <summary>
            /// Sets all shader uniforms for the InsectEye shader and renders the effect to the screen.
            /// </summary>
            /// <param name="context">The post-processing render context.</param>
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

