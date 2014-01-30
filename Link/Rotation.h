#include "stdafx.h"
#include <d3dx9.h>
#include <d3dx9math.h>

#pragma comment(lib,"d3d9.lib")
#pragma comment(lib,"d3dx9.lib")

D3DXMATRIX *dhMatrixTranslation(D3DXMATRIX *p_out,float p_x,float p_y,float p_z);
D3DXMATRIX *dhMatrixRotationY(D3DXMATRIX *pOut, float angle );

D3DXMATRIX *dhMatrixRotationY(D3DXMATRIX *pOut, float angle ){
	float my_sin, my_cos;


	my_sin=(float)sin(angle);
	my_cos=(float)cos(angle);

	pOut->_11 = my_cos;  pOut->_12 =  0.0f;   pOut->_13 = -my_sin; pOut->_14 = 0.0f;
	pOut->_21 = 0.0f;    pOut->_22 =  1.0f;   pOut->_23 = 0.0f;    pOut->_24 = 0.0f;
	pOut->_31 = my_sin;  pOut->_32 =  0.0f;   pOut->_33 = my_cos;  pOut->_34 = 0.0f;
	pOut->_41 = 0.0f;    pOut->_42 =  0.0f;   pOut->_43 = 0.0f;    pOut->_44 = 1.0f;

	return pOut;
}

D3DXMATRIX *dhMatrixTranslation(D3DXMATRIX *p_out,float p_x,float p_y,float p_z){

   p_out->_11 = 1.0f;    p_out->_12 = 0.0f;    p_out->_13 = 0.0f;    p_out->_14 = 0.0f;
   p_out->_21 = 0.0f;    p_out->_22 = 1.0f;    p_out->_23 = 0.0f;    p_out->_24 = 0.0f;
   p_out->_31 = 0.0f;    p_out->_32 = 0.0f;    p_out->_33 = 1.0f;    p_out->_34 = 0.0f;
   p_out->_41 = p_x;     p_out->_42 = p_y;     p_out->_43 = p_z;     p_out->_44 = 1.0f;

   return p_out;
}

