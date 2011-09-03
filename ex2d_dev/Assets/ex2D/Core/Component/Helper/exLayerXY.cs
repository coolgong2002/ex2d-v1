// ======================================================================================
// File         : exLayerXY.cs
// Author       : Wu Jie 
// Last Change  : 09/01/2011 | 15:52:47 PM | Thursday,September
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

///////////////////////////////////////////////////////////////////////////////
// defines
///////////////////////////////////////////////////////////////////////////////

[AddComponentMenu("ex2D Helper/2D Layer XY")]
public class exLayerXY : exLayer2D {

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    override public float CalculateDepth ( Camera _cam ) {
        if ( _cam == null )
            return 0.0f;
        float dist = _cam.farClipPlane - _cam.nearClipPlane;
        float unitLayer = dist/MAX_LAYER;
        return ((float)layer_ + bias_) * unitLayer + _cam.transform.position.z + _cam.nearClipPlane;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    override public void UpdateTransformDepth () { 
        if ( Mathf.Approximately(depth_, transform.position.z) == false ) {
            transform.position = new Vector3( transform.position.x,
                                              transform.position.y,
                                              depth_ );
        }
    }

}