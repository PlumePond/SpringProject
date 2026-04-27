using Microsoft.Xna.Framework;
using SpringProject.Core.Audio;
using SpringProject.Core.Debugging;
using SpringProject.Core.UI;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.Editor;

public class PointerTool(GridPlacement placement) : Tool(placement)
{
    public override CursorType CursorType => CursorType.Pointer;

    float _dragTimer = 0.0f;
    const float TIME_PRESSED_UNTIL_DRAGGING = 0.2f;
    LevelObject _objectToDrag;
    bool _dragging = false;

    AudioComposite _invalidSound => AudioManager.Get("invalid");

    public override void Update(GameTime gameTime)
    {
        if (_objectToDrag != null)
        {
            _dragTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        if (Input.Get("select").Released && _objectToDrag != null)
        {
            if (_dragging)
            {
                EndDrag();
            }
            else
            {
                Cursor.EndPress();
                _placement.Select(_objectToDrag);
                _objectToDrag = null;
            }
        }

        // dont drag if the object stops being hovered
        if (_placement.hoveredObject == null && _objectToDrag != null && !_dragging)
        {
            Cursor.EndPress();
            _placement.Select(_objectToDrag);
            _objectToDrag = null;
        }

        if (_dragTimer >= TIME_PRESSED_UNTIL_DRAGGING && !_dragging && _placement.hoveredObject != null)
        {
            if (_objectToDrag == _placement.hoveredObject)
            {
                BeginDrag();
            }
        }

        // drag
        if (_dragging && _objectToDrag != null)
        {
            int snapSize = _objectToDrag.data.enforceGrid ? (int)SnapSize.Whole : (int)_placement.SnapSize;
            Point snappedPosition = _placement.CalculateSmartPlacement(_objectToDrag.data, _placement.MousePos, snapSize, _placement.Rotation, out bool invalidPlacement, _objectToDrag);
            
            if (!invalidPlacement)
            {
                _objectToDrag.SetPosition(snappedPosition);
            }
        }

        if (Input.Get("select").Released)
        {
            Debug.Log($"Select released. Dragging: {_dragging}. Object to drag is null: {_objectToDrag == null}");
        }
    }

    public override void PressPrimary(LevelObject hovered, Point mousePos, bool swipe)
    {
        if (swipe) return;

        if (_placement.selectedObject == hovered)
        {
            _placement.Deselect();
        }
        else
        {
            Cursor.BeginPress();
            TryDrag(hovered);
        }
    }

    public override void PressEmpty(Point mousePos, bool swipe)
    {
        if (_placement.SelectedObjectData != null && _placement.CanPlaceObject && _placement.hoveredObject == null)
        {
            if (_placement.SelectedObjectData.restrictPlacement)
            {
                if (_placement.LastPreviewInvalid)
                {
                    if (!swipe)
                    {
                        _invalidSound.Play();
                    }
                }
                else
                {
                    _placement.PlaceObject(_placement.SelectedObjectData, _placement.LastPreviewPosition, _placement.Grid.activeLayer);
                }
            }
            else
            {
                int snapSize = _placement.SelectedObjectData.enforceGrid ? (int)SnapSize.Whole : (int)_placement.SnapSize;
                _placement.PlaceObject(_placement.SelectedObjectData, _placement.SnapToGrid(mousePos, snapSize), _placement.Grid.activeLayer);
            }

            if (swipe)
            {
                _placement.JustPlaced = false;
            }
        }
    }

    public override void PressSecondary(LevelObject hovered, Point mousePos, bool swipe)
    {
        _placement.RemoveObject(hovered);
        _placement.CanPlaceObject = true;
    }

    public override void Hold(GameTime gameTime, LevelObject hovered)
    {
        
    }

    public override void Release()
    {
        
    }

    void TryDrag(LevelObject levelObject)
    {
        if (_objectToDrag == levelObject) return; // don't reset if already tracking
        _dragTimer = 0.0f;
        _objectToDrag = levelObject;
    }

    void BeginDrag()
    {
        _dragging = true;
        Cursor.BeginGrab();
    }

    void EndDrag()
    {
        _objectToDrag = null;
        _dragging = false;
        Cursor.EndGrab();
    }
}