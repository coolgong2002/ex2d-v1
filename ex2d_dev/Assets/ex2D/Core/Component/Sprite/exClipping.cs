// ======================================================================================
// File         : exClipping.cs
// Author       : Wu Jie 
// Last Change  : 03/05/2012 | 19:19:47 PM | Monday,March
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
// defines
///////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////////////
///
/// A component handles the a list of exPlane GameObjects, clip them
/// to the boundingRect.
///
///////////////////////////////////////////////////////////////////////////////

[ExecuteInEditMode]
[AddComponentMenu("ex2D Sprite/Clipping")]
public class exClipping : exPlane {

    // ------------------------------------------------------------------ 
    [SerializeField] protected float width_ = 100.0f;
    /// the width of the soft-clip
    // ------------------------------------------------------------------ 

    public float width {
        get { return width_; }
        set {
            if ( width_ != value ) {
                width_ = Mathf.Max(value, 0.0f);
                updateFlags |= UpdateFlags.Vertex;
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected float height_ = 100.0f;
    /// the height of the soft-clip
    // ------------------------------------------------------------------ 

    public float height {
        get { return height_; }
        set {
            if ( height_ != value ) {
                height_ = Mathf.Max(value, 0.0f);
                updateFlags |= UpdateFlags.Vertex;
            }
        }
    }

    // ------------------------------------------------------------------ 
    /// the list of the planes to clip
    // ------------------------------------------------------------------ 

    public List<exPlane> planes = new List<exPlane>();

    // ------------------------------------------------------------------ 
    /// the clipped rect, if the clipping plane is a child of another soft-clip plane
    // ------------------------------------------------------------------ 

    public Rect clippedRect { get; protected set; }

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    /// update the list of planes to clip
    // ------------------------------------------------------------------ 

    public void UpdateClipList () {
        planes.Clear();
        if ( transform.childCount > 0 )
            RecursivelyAddToClip (transform);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void RecursivelyAddToClip ( Transform _t ) {
        foreach ( Transform child in _t ) {
            exPlane plane = child.GetComponent<exPlane>();
            if ( plane != null ) {
                planes.Add(plane);
                exClipping clipPlane = plane as exClipping;
                // if this is a clip plane, add child to it 
                if ( clipPlane != null ) {
                    clipPlane.UpdateClipList ();
                    continue;
                }
                else {
                    Renderer renderer = child.renderer;
                    if ( renderer != null ) {
                        renderer.material.shader = Shader.Find("ex2D/Alpha Blended (Clipping)");
                    }
                }
            }
            RecursivelyAddToClip (child);
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// Awake functoin inherit from exPlane.
    // ------------------------------------------------------------------ 

    protected new void Awake () {
        base.Awake();
        updateFlags |= UpdateFlags.Vertex;
        Commit();
        CommitMaterialProperties();

        spriteMng.AddToClippingList(this);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected new void OnDestroy () {
        base.OnDestroy();

        if ( spriteMng != null ) {
            spriteMng_.RemoveFromClippingList(this);
        }
    }

    // ------------------------------------------------------------------ 
    /// OnEnable functoin inherit from exPlane.
    /// When enabled set to true, it will enable all the item in the planes
    // ------------------------------------------------------------------ 

    protected new void OnEnable () {
        base.OnEnable();

        for ( int i = 0; i < planes.Count; ++i ) {
            exPlane p = planes[i];
            if ( p == null ) {
                planes.RemoveAt(i);
                --i;
                continue;
            }
            p.enabled = true;
        }
    }

    // ------------------------------------------------------------------ 
    /// OnDisable functoin inherit from exPlane.
    /// When enabled set to false, it will disable all the item in the planes
    // ------------------------------------------------------------------ 

    protected new void OnDisable () {
        base.OnDisable();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public override void Commit () {
        if ( (updateFlags & UpdateFlags.Vertex) != 0 ) {
            //
            float halfWidth = width_ * 0.5f;
            float halfHeight = height_ * 0.5f;
            float offsetX = 0.0f;
            float offsetY = 0.0f;

            //
            switch ( anchor ) {
            case Anchor.TopLeft     : offsetX = -halfWidth;   offsetY = -halfHeight;  break;
            case Anchor.TopCenter   : offsetX = 0.0f;         offsetY = -halfHeight;  break;
            case Anchor.TopRight    : offsetX = halfWidth;    offsetY = -halfHeight;  break;

            case Anchor.MidLeft     : offsetX = -halfWidth;   offsetY = 0.0f;         break;
            case Anchor.MidCenter   : offsetX = 0.0f;         offsetY = 0.0f;         break;
            case Anchor.MidRight    : offsetX = halfWidth;    offsetY = 0.0f;         break;

            case Anchor.BotLeft     : offsetX = -halfWidth;   offsetY = halfHeight;   break;
            case Anchor.BotCenter   : offsetX = 0.0f;         offsetY = halfHeight;   break;
            case Anchor.BotRight    : offsetX = halfWidth;    offsetY = halfHeight;   break;

            default                 : offsetX = 0.0f;         offsetY = 0.0f;         break;
            }

            //
            boundingRect = new Rect( -offsetX - halfWidth, 
                                      offsetY - halfHeight,
                                      width_, 
                                      height_ );

            // TODO: child clip { 
            // // do clip
            // if ( clipInfo_.clipped ) {
            //     clippedRect = new Rect( boundingRect.x + clipInfo_.left * boundingRect.width, 
            //                             boundingRect.y + clipInfo_.top * boundingRect.height, 
            //                             (1.0f - clipInfo_.left - clipInfo_.right) * boundingRect.width,
            //                             (1.0f - clipInfo_.top - clipInfo_.bottom) * boundingRect.height
            //                           ); 
            // }
            // else {
            //     clippedRect = boundingRect;
            // }
            // } TODO end 
            clippedRect = boundingRect;

            if ( collisionHelper ) 
                collisionHelper.UpdateCollider();
        }

        //
        updateFlags = UpdateFlags.None;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void CommitMaterialProperties () {
        Vector4 rect = new Vector4 ( clippedRect.center.x + transform.position.x, 
                                     clippedRect.center.y + transform.position.y, 
                                     clippedRect.width, 
                                     clippedRect.height ); 
        Matrix4x4 rot = Matrix4x4.TRS ( Vector3.zero, transform.rotation, Vector3.one );
        rot = rot.inverse;

        for ( int i = 0; i < planes.Count; ++i ) {
            exPlane p = planes[i];
            Renderer r = p.renderer;
            r.sharedMaterial.SetVector ( "_ClipRect", rect );
            r.sharedMaterial.SetMatrix ( "_ClipRotation", rot );
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnDrawGizmos () {
        //
        Vector3 center_v3 = transform.position;
        Vector3 size_v3 = Vector3.zero;
        float halfWidth = width_ * 0.5f;
        float halfHeight = height_ * 0.5f;
        float offsetX = 0.0f;
        float offsetY = 0.0f;

        //
        switch ( anchor ) {
        case Anchor.TopLeft     : offsetX = -halfWidth;   offsetY = -halfHeight;  break;
        case Anchor.TopCenter   : offsetX = 0.0f;         offsetY = -halfHeight;  break;
        case Anchor.TopRight    : offsetX = halfWidth;    offsetY = -halfHeight;  break;

        case Anchor.MidLeft     : offsetX = -halfWidth;   offsetY = 0.0f;         break;
        case Anchor.MidCenter   : offsetX = 0.0f;         offsetY = 0.0f;         break;
        case Anchor.MidRight    : offsetX = halfWidth;    offsetY = 0.0f;         break;

        case Anchor.BotLeft     : offsetX = -halfWidth;   offsetY = halfHeight;   break;
        case Anchor.BotCenter   : offsetX = 0.0f;         offsetY = halfHeight;   break;
        case Anchor.BotRight    : offsetX = halfWidth;    offsetY = halfHeight;   break;

        default                 : offsetX = 0.0f;         offsetY = 0.0f;         break;
        }

        //
        float x = offset_.x - offsetX;
        float y = offset_.y + offsetY;

        center_v3 += new Vector3( x, y, 0.0f );
        size_v3 = new Vector3 ( width_, height_, 0.0f );

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube ( center_v3, size_v3 );
        // Gizmos.color = new Color ( 1.0f, 1.0f, 0.0f, 0.0001f ); // this is very hack
        // Gizmos.DrawCube ( center_v3, new Vector3 ( size.x, size.y, 0.0f ) );
    }
}

