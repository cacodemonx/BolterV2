using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace Bolter_XIV
{
    unsafe public class NativeDx : GameInput
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct D3DXMATRIX
        {
            private Single _11;
            private Single _12;
            private Single _13;
            private Single _14;
            private Single _21;
            private Single _22;
            private Single _23;
            private Single _24;
            public Single _31;
            private Single _32;
            public Single _33;
            private Single _34;
            private Single _41;
            private Single _42;
            private Single _43;
            private Single _44;
        }

        [DllImport("d3dx9_43.dll", EntryPoint = "D3DXMatrixRotationY", CallingConvention = CallingConvention.StdCall,
            SetLastError = false)]
        private static extern D3DXMATRIX* D3DXMatrixRotationY(
             D3DXMATRIX* pOut,
             Single Angle
            );

        [DllImport("d3dx9_43.dll", EntryPoint = "D3DXMatrixTranslation", CallingConvention = CallingConvention.StdCall,
            SetLastError = false)]
        private static extern D3DXMATRIX* D3DXMatrixTranslation(
             D3DXMATRIX* pOut,
             Single x,
             Single y,
             Single z
            );

        [DllImport("d3dx9_43.dll", EntryPoint = "D3DXMatrixMultiply", CallingConvention = CallingConvention.StdCall,
            SetLastError = false)]
        private static extern D3DXMATRIX* D3DXMatrixMultiply(
             D3DXMATRIX* pOut,
             D3DXMATRIX* pM1,
             D3DXMATRIX* pM2
            );

        protected static D3DXVECTOR2 GetNewVector(float heading)
        {
            var updatedMatrix = new D3DXMATRIX();
            var transMatrix = new D3DXMATRIX();
            var yRotationMatrix = new D3DXMATRIX();

            //Set up the rotation matrix for the player model
            D3DXMatrixRotationY(&yRotationMatrix, heading/2);

            //Set up the translation matrix 
            D3DXMatrixTranslation(&transMatrix, 0.0f, 0.0f, 0.0f);

            //Combine out matrices
            D3DXMatrixMultiply(&updatedMatrix, &yRotationMatrix, &transMatrix);

            //Return new vector for player matrix.
            return new D3DXVECTOR2(updatedMatrix._31, updatedMatrix._33);

        }

    }

    [Serializable]
    public class D3DXVECTOR2
    {
        [XmlAttribute("X")] 
        public float x;
        [XmlAttribute("Y")] 
        public float y;

        public D3DXVECTOR2()
        {
            
        }

        public D3DXVECTOR2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        
    }
}