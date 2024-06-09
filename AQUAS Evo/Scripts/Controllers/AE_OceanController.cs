using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AquasEvo
{
    [ExecuteAlways]
    public class AE_OceanController : MonoBehaviour
    {
        public AE_HeightFieldGenerator m_heightFieldGenerator;

        #region debug
        public Texture2D m_debugTex;
        public RenderTexture m_debugRenderTex;

        public ComputeShader m_shader;
        #endregion

        private void Update()
        {
            if (m_heightFieldGenerator == null)
            {
                m_heightFieldGenerator = new AE_HeightFieldGenerator();
                m_heightFieldGenerator.m_oceanControllerDEBUG = this;
                return;
            }
            else m_heightFieldGenerator.Update();
        }

        private void OnGUI()
        {
            #region debugging
            DebugDrawer();
            #endregion
        }

        private void DebugDrawer()
        {
            if (m_debugRenderTex == null) return;

            if (Event.current.type.Equals(EventType.Repaint))
            {
                Graphics.DrawTexture(new Rect(0, 0, 512, 512), m_debugRenderTex);
                //GUI.DrawTexture(new Rect(0, 0, 512, 512), m_debugRenderTex);
            }
            
        }
    }
}