using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Inventor;
using QubeItTools.General;
using QubeItTools.Interfaces;

namespace QubeItTools.ClientModel
{
    /// <summary>
    /// This is the base class for the logic that executes the drawing and interaction of creating
    /// the rectangle. Due to the different locals of the origin point of each of the different types
    /// of rectangles, much of the logic had to be done in the sub-classes.
    /// </summary>
    public abstract class Rectangle
    {
        #region Member Variables
        //Application object
        protected Inventor.Application _inventorApplication;

        //Change Processor to be used for undo and redo operations
        protected QubeItToolsChangeProcessor _changeProcessor;

        //Create a  List of SketchLines for use to build the rectangle
        protected List<SketchLine> _rectangleLines;

        //Reference to the construction line toggle status
        protected ButtonDefinition _constructionLineButton;
        protected ButtonDefinition _centerLineButton;
        protected ButtonDefinition _sketchOnlyButton;

        //Create some sketch entity variables for use with the rectangle
        protected SketchPoint _rectangleOriginSketchPoint;
        protected object _firstSelectedSketchEntity;
        protected object _firstInferredIntersectedSketchEntity;
        protected object _secondSelectedSketchEntity;
        protected object _secondInferredIntersectedSketchEntity;
        protected PointInference _inferredOriginPoint;
        protected PointInference _inferredFinalPositionPoint;
        protected SketchPoint _verticalMidPointAlign;
        protected SketchPoint _horizontalMidPointAlign;

        //Sketch points for the point that the user potentially picks
        protected SketchPoint _pickedSketchPoint;

        //Create a planar sketch and drawing sketch variable
        protected PlanarSketch _planarSketch;
        protected DrawingSketch _drawingSketch;

        //Create 4 Point2d objects
        protected Point2d _pickedPoint2d;
        protected Point2d _upperRightPoint2d;
        protected Point2d _upperLeftPoint2d;
        protected Point2d _lowerLeftPoint2d;
        protected Point2d _lowerRightPoint2d;

        //Create a Point object to store the final location of the mouse when the rectangle gets locked
        protected Point _finalPosition;

        //Create a reference to a rectangle button
        protected IInventorButton _rectangleButton;

        //create the Interaction events object which gives access to sublevel objects such as mouse events etc.
        protected Inventor.InteractionEvents _interactionEvents;

        //Create the Interaction Graphics objects needed to interactively draw the rectangle as the user drags his/her mouse around
        protected Double[] _rectanglePointCoords;
        protected GraphicsIndexSet _rectangleIndexSet;
        protected InteractionGraphics _rectangleInteractionGraphics;
        protected ClientGraphics _rectangleClientGraphics;
        protected GraphicsDataSets _rectangleGraphicsDataSets;
        protected GraphicsCoordinateSet _rectangleCoordSet;
        protected GraphicsNode _rectangleLineNode;
        protected LineStripGraphics _rectangleLineStripGraphics;
        protected GraphicsColorSet _rectangleGraphicsColorSet;
        protected Color _activeDOFFreeColor;

        //Create a variable to hold the MouseEvents Object
        protected Inventor.MouseEvents _mouseEvents;

        //Mouse Event Delegates
        protected Inventor.MouseEventsSink_OnMouseClickEventHandler _onMouseClick_Delegate;
        protected Inventor.MouseEventsSink_OnMouseMoveEventHandler _onMouseMove_Delegate;

        //User Input Events variable
        protected UserInputEvents _userInputEvents;

        //User Input Event Delegates
        protected UserInputEventsSink_OnContextMenuEventHandler _userInputEvents_OnContextMenuDelegate;

        //Interaction variables
        protected Inventor.InteractionEventsSink_OnTerminateEventHandler _onTerminate_Delegate;
        #endregion

        /// <summary>
        /// When the appropriate rectangle button is clicked the button's OnExecute event handler
        /// calls this method to hook up the mouse click event which will wait for the user to select 
        /// a point from which to start the drawing of the rectangle
        /// </summary>
        public virtual void StartRectangleInteraction()
        {   
            _interactionEvents = _inventorApplication.CommandManager.CreateInteractionEvents();
            
            _rectangleInteractionGraphics = _interactionEvents.InteractionGraphics;
            _rectangleClientGraphics = _rectangleInteractionGraphics.OverlayClientGraphics;
            _rectangleLineNode = _rectangleClientGraphics.AddNode(1);
            _rectangleGraphicsDataSets = _rectangleInteractionGraphics.GraphicsDataSets;
            _rectangleCoordSet = _rectangleGraphicsDataSets.CreateCoordinateSet(1);
            _rectangleIndexSet = _rectangleGraphicsDataSets.CreateIndexSet(1);

            _onTerminate_Delegate = new InteractionEventsSink_OnTerminateEventHandler(StopInteraction);
            _interactionEvents.OnTerminate += _onTerminate_Delegate;

            _userInputEvents = _inventorApplication.CommandManager.UserInputEvents;
            _userInputEvents_OnContextMenuDelegate = new UserInputEventsSink_OnContextMenuEventHandler(UserInputEvents_OnContextMenu);
            _userInputEvents.OnContextMenu += _userInputEvents_OnContextMenuDelegate;

            _mouseEvents = _interactionEvents.MouseEvents;
            _mouseEvents.PointInferenceEnabled = true;
            _onMouseClick_Delegate = new MouseEventsSink_OnMouseClickEventHandler(OnMouseClick_CreateRectangle);
            _mouseEvents.OnMouseClick += _onMouseClick_Delegate;

            _interactionEvents.StatusBarText = "Select a Point from which to start the rectangle";

            _interactionEvents.AllowCommandAliases = true;

            _interactionEvents.Start();
        }

        /// <summary>
        /// This is the Event handler for the right click menu.
        /// I'm merely adding an Esc control to the top of the context menu in order
        /// to give the user the ability to intuitively exit the command. There's a code sample in
        /// the API help that prints out all the ControlDefinitions inside Inventor. I used that to 
        /// find the aptly named "Done" command to call from the ControlDefinitions collection.
        /// </summary>
        /// <param name="SelectionDevice"></param>
        /// <param name="AdditionalInfo"></param>
        /// <param name="CommandBar"></param>
        protected virtual void UserInputEvents_OnContextMenu(SelectionDeviceEnum SelectionDevice, NameValueMap AdditionalInfo,
                                                                CommandBar CommandBar)
        {
            ButtonDefinition _escapeControl = (ButtonDefinition)_inventorApplication.CommandManager.ControlDefinitions["Done"];
            CommandBar.Controls.AddButton(ButtonDefinition: _escapeControl);
            int lastControl = CommandBar.Controls.Count;
            CommandBar.Controls[lastControl].Delete();
        }

        /// <summary>
        /// When the user clicks on a point in the model, this method runs and creates an interaction graphics rectangle. 
        /// It also subscribes to the Mouse Move event to allow for the size of the rectangle to be driven by the location 
        /// of the mouse on the sketch. Ultimately, this method waits for the next click event, in which case it calls a 
        /// new method to actually create the sketched rectangle.  It has separate paths to react to whether it's a part
        /// sketch or a drawing sketch.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="shiftKeys"></param>
        /// <param name="modelPosition"></param>
        /// <param name="viewPosition"></param>
        /// <param name="view"></param>
        protected void OnMouseClick_CreateRectangle(MouseButtonEnum button, ShiftStateEnum shiftKeys, Point modelPosition,
                                                Point2d viewPosition, Inventor.View view)
        {
            try
            {
                if (button == MouseButtonEnum.kLeftMouseButton)
                {
                    _onMouseMove_Delegate = new MouseEventsSink_OnMouseMoveEventHandler(OnMouseMove_DragCornerOfRectangle);
                    _mouseEvents.OnMouseClick -= _onMouseClick_Delegate;
                    _onMouseClick_Delegate = null;
                    _userInputEvents.OnContextMenu -= _userInputEvents_OnContextMenuDelegate;
                    _userInputEvents_OnContextMenuDelegate = null;

                    if (_inventorApplication.ActiveEditObject is PlanarSketch)
                    {
                        _planarSketch = (PlanarSketch)_inventorApplication.ActiveEditObject;

                        if (_mouseEvents.PointInferences.Count > 0)
                        {
                            _inferredOriginPoint = _mouseEvents.PointInferences[1];

                            switch (_inferredOriginPoint.InferenceType)
                            {
                                case PointInferenceEnum.kPtAtIntersection:
                                    _firstSelectedSketchEntity = (SketchEntity)_inferredOriginPoint.Entity[1];
                                    _firstInferredIntersectedSketchEntity = (SketchEntity)_inferredOriginPoint.Entity[2];
                                    break;
                                case PointInferenceEnum.kPtOnCurve:
                                    _firstSelectedSketchEntity = (SketchEntity)_inferredOriginPoint.Entity[1];
                                    break;
                                case PointInferenceEnum.kPtOnPt:
                                    _firstSelectedSketchEntity = (SketchPoint)_inferredOriginPoint.Entity[1];
                                    _pickedSketchPoint = (SketchPoint)_firstSelectedSketchEntity;
                                    _rectangleOriginSketchPoint = _pickedSketchPoint;
                                    break;
                                case PointInferenceEnum.kPtAtMidPoint:
                                    _firstSelectedSketchEntity = (SketchLine)_inferredOriginPoint.Entity[1];
                                    break;
                            }
                        }

                        _pickedPoint2d = _planarSketch.ModelToSketchSpace(modelPosition);

                        DrawInteractionRectangle();

                        _mouseEvents.OnMouseMove += _onMouseMove_Delegate;
                        _mouseEvents.MouseMoveEnabled = true;
                    }
                    else if (_inventorApplication.ActiveEditObject is DrawingSketch)
                    {
                        _drawingSketch = (DrawingSketch)_inventorApplication.ActiveEditObject;

                        if (_mouseEvents.PointInferences.Count > 0)
                        {
                            _inferredOriginPoint = _mouseEvents.PointInferences[1];

                            switch (_inferredOriginPoint.InferenceType)
                            {
                                case PointInferenceEnum.kPtAtIntersection:
                                    _firstSelectedSketchEntity = (SketchEntity)_inferredOriginPoint.Entity[1];
                                    _firstInferredIntersectedSketchEntity = (SketchEntity)_inferredOriginPoint.Entity[2];
                                    break;
                                case PointInferenceEnum.kPtOnCurve:
                                    _firstSelectedSketchEntity = (SketchEntity)_inferredOriginPoint.Entity[1];
                                    break;
                                case PointInferenceEnum.kPtOnPt:
                                    _firstSelectedSketchEntity = (SketchPoint)_inferredOriginPoint.Entity[1];
                                    _pickedSketchPoint = (SketchPoint)_firstSelectedSketchEntity;
                                    _rectangleOriginSketchPoint = _pickedSketchPoint;
                                    break;
                                case PointInferenceEnum.kPtAtMidPoint:
                                    _firstSelectedSketchEntity = (SketchLine)_inferredOriginPoint.Entity[1];
                                    break;
                            }
                        }
                        PointToPoint2d(modelPosition, out _pickedPoint2d); 

                        DrawInteractionRectangle();

                        _mouseEvents.OnMouseMove += _onMouseMove_Delegate;
                        _mouseEvents.MouseMoveEnabled = true;
                    }
                    else
                        MessageBox.Show("You must be editing a sketch in order to use this tool.");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

        }

        /// <summary>
        /// Draws the interaction graphics rectangle. Again, two paths for the part sketch or drawing sketch, with 
        /// the drawing sketch not getting any color overrides due to how it blends into the "paper". Instead it 
        /// defaults to black in the drawing sketch.  The part sketch gets a color from the Degrees of Freedom color
        /// schema.
        /// </summary>
        protected void DrawInteractionRectangle()
        {
            _upperRightPoint2d.X = _pickedPoint2d.X + 0.0001;
            _upperRightPoint2d.Y = _pickedPoint2d.Y + 0.0001;

            PositionPoint2dObjects();

            WriteToInteractionPointCoords();

            _rectangleCoordSet.PutCoordinates(ref _rectanglePointCoords);

            if (_planarSketch != null)
            {
                _activeDOFFreeColor = _inventorApplication.ActiveColorScheme.DOFFreeColor;
                _rectangleGraphicsColorSet = _rectangleGraphicsDataSets.CreateColorSet(1);
                _rectangleGraphicsColorSet.Add(1, _activeDOFFreeColor.Red, _activeDOFFreeColor.Green, _activeDOFFreeColor.Blue);

                _rectangleIndexSet.Add(1, 1);
                _rectangleIndexSet.Add(2, 2);
                _rectangleIndexSet.Add(3, 3);
                _rectangleIndexSet.Add(4, 4);
                _rectangleIndexSet.Add(5, 1);

                _rectangleLineStripGraphics = _rectangleLineNode.AddLineStripGraphics();
                _rectangleLineStripGraphics.CoordinateSet = _rectangleCoordSet;
                _rectangleLineStripGraphics.CoordinateIndexSet = _rectangleIndexSet;
                _rectangleLineStripGraphics.ColorSet = _rectangleGraphicsColorSet;
            }
            else if (_drawingSketch != null)
            {
                _rectangleIndexSet.Add(1, 1);
                _rectangleIndexSet.Add(2, 2);
                _rectangleIndexSet.Add(3, 3);
                _rectangleIndexSet.Add(4, 4);
                _rectangleIndexSet.Add(5, 1);

                _rectangleLineStripGraphics = _rectangleLineNode.AddLineStripGraphics();
                _rectangleLineStripGraphics.CoordinateSet = _rectangleCoordSet;
                _rectangleLineStripGraphics.CoordinateIndexSet = _rectangleIndexSet;
            }

            _rectangleInteractionGraphics.UpdateOverlayGraphics(_inventorApplication.ActiveView);
        }

        protected abstract void PositionPoint2dObjects();

        /// <summary>
        /// The rectangle is based on the location of Point2d objects and this method writes the X and Y 
        /// values of those objects to the array used in creating the interaction graphics rectangle
        /// </summary>
        protected void WriteToInteractionPointCoords()
        {
            if (_rectanglePointCoords == null)
            {
                _rectanglePointCoords = new Double[12]; /* new double[12];*/
            }

            List<Point> graphicPoints = new List<Point>();

            if (_planarSketch != null)
            {
                //In a part document, the sketch could be located anywhere in 3D space so this algorithm
                //provides a mechanism to make sure the interaction graphics gets drawn on the same plane
                //and in the same location as the sketch.
                graphicPoints.Add(_planarSketch.SketchToModelSpace(_upperRightPoint2d));
                graphicPoints.Add(_planarSketch.SketchToModelSpace(_upperLeftPoint2d));
                graphicPoints.Add(_planarSketch.SketchToModelSpace(_lowerLeftPoint2d));
                graphicPoints.Add(_planarSketch.SketchToModelSpace(_lowerRightPoint2d));

                _rectanglePointCoords[0] = graphicPoints[0].X;
                _rectanglePointCoords[1] = graphicPoints[0].Y;
                _rectanglePointCoords[2] = graphicPoints[0].Z;

                _rectanglePointCoords[3] = graphicPoints[1].X;
                _rectanglePointCoords[4] = graphicPoints[1].Y;
                _rectanglePointCoords[5] = graphicPoints[1].Z;

                _rectanglePointCoords[6] = graphicPoints[2].X;
                _rectanglePointCoords[7] = graphicPoints[2].Y;
                _rectanglePointCoords[8] = graphicPoints[2].Z;

                _rectanglePointCoords[9] = graphicPoints[3].X;
                _rectanglePointCoords[10] = graphicPoints[3].Y;
                _rectanglePointCoords[11] = graphicPoints[3].Z;

                graphicPoints.Clear();
            }
            else if (_drawingSketch != null)
            {
                _rectanglePointCoords[0] = _upperRightPoint2d.X;
                _rectanglePointCoords[1] = _upperRightPoint2d.Y;
                _rectanglePointCoords[2] = 0.0;

                _rectanglePointCoords[3] = _upperLeftPoint2d.X;
                _rectanglePointCoords[4] = _upperLeftPoint2d.Y;
                _rectanglePointCoords[5] = 0.0;

                _rectanglePointCoords[6] = _lowerLeftPoint2d.X;
                _rectanglePointCoords[7] = _lowerLeftPoint2d.Y;
                _rectanglePointCoords[8] = 0.0;

                _rectanglePointCoords[9] = _lowerRightPoint2d.X;
                _rectanglePointCoords[10] = _lowerRightPoint2d.Y;
                _rectanglePointCoords[11] = 0.0;
            }


        }

        /// <summary>
        /// Method that adjusts the points that make up the rectangle according to the "new" location of
        /// the upper Right Point2d
        /// </summary>
        /// <param name="modelPosition"></param>
        protected void AdjustRectanglePosition(Point modelPosition)
        {
            if (_planarSketch != null)
            {
                _upperRightPoint2d = _planarSketch.ModelToSketchSpace(modelPosition);
            }
            else if (_drawingSketch != null)
            {
                PointToPoint2d(modelPosition, out _upperRightPoint2d);
            }

            PositionPoint2dObjects();

            WriteToInteractionPointCoords();

            _rectangleCoordSet.PutCoordinates(_rectanglePointCoords);
            _rectangleLineStripGraphics.CoordinateSet = _rectangleCoordSet;
            _rectangleLineStripGraphics.CoordinateIndexSet = _rectangleIndexSet;

            _rectangleInteractionGraphics.UpdateOverlayGraphics(_inventorApplication.ActiveView);
        }

        /// <summary>
        /// This method reacts to the Mouse Move event and calls a method to reposition the points that make
        /// up the rectangle and also subscribes to the mouse click event for when the rectangle is to be 
        /// instantiated by the user.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="shiftKeys"></param>
        /// <param name="modelPosition"></param>
        /// <param name="viewPosition"></param>
        /// <param name="view"></param>
        protected void OnMouseMove_DragCornerOfRectangle(MouseButtonEnum button, ShiftStateEnum shiftKeys, Point modelPosition,
                                            Point2d viewPosition, Inventor.View view)
        {
            AdjustRectanglePosition(modelPosition);


            if (_onMouseClick_Delegate == null)
            {
                _onMouseClick_Delegate = new MouseEventsSink_OnMouseClickEventHandler(OnMouseClick_LockTheRectangle);
                _mouseEvents.OnMouseClick += _onMouseClick_Delegate;

                _userInputEvents_OnContextMenuDelegate += new UserInputEventsSink_OnContextMenuEventHandler(UserInputEvents_OnContextMenu);
                _userInputEvents.OnContextMenu += _userInputEvents_OnContextMenuDelegate;
            }
        }

        /// <summary>
        /// Method that reacts to the click event by the user to instantiate the rectangle. This method starts 
        /// the ChangeProcessor which encapsulates the drawing of the rectangle into one undo/redo mechanism.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="shiftKeys"></param>
        /// <param name="modelPosition"></param>
        /// <param name="viewPosition"></param>
        /// <param name="view"></param>
        protected void OnMouseClick_LockTheRectangle(MouseButtonEnum button, ShiftStateEnum shiftKeys, Point modelPosition,
                                        Point2d viewPosition, Inventor.View view)
        {
            try
            {
                if (button == MouseButtonEnum.kLeftMouseButton)
                {
                    _mouseEvents.MouseMoveEnabled = false;

                    if (_mouseEvents.PointInferences.Count > 0)
                    {
                        _inferredFinalPositionPoint = _mouseEvents.PointInferences[1];
                    }

                    _finalPosition = modelPosition;

                    _changeProcessor.Connect();

                    ResetRectangle();

                    _rectangleButton.ButtonPressed = false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// Method that creates the sketched rectangle
        /// </summary>
        protected void InstantiateRectangle()
        {
            if (_drawingSketch != null)
            {
                AdjustDrawingDocumentPoints();
            }

            DrawRectangle();

            ConstrainRectangle();
        }

        /// <summary>
        /// Transforms the point2d objects to scale of the sketch in order to draw the rectangle to the proper scale
        /// as shown in the interaction graphics.
        /// </summary>
        protected void AdjustDrawingDocumentPoints()
        {
            TransientGeometry transGeom = _inventorApplication.TransientGeometry;
            Point2d tempPoint2d = transGeom.CreatePoint2d();
            Point2d tempFinalPosition = transGeom.CreatePoint2d();

            tempPoint2d = _pickedPoint2d;
            _pickedPoint2d = _drawingSketch.SheetToSketchSpace(tempPoint2d);

            tempFinalPosition.X = _finalPosition.X;
            tempFinalPosition.Y = _finalPosition.Y;
            _upperRightPoint2d = _drawingSketch.SheetToSketchSpace(tempFinalPosition);

            PositionPoint2dObjects();
        }

        /// <summary>
        /// Method that draws the rectangle based on the user inputs
        /// </summary>
        protected virtual void DrawRectangle()
        {
            if (_planarSketch != null)
            {
                _rectangleLines = new List<SketchLine>();

                _rectangleLines.Add(_planarSketch.SketchLines.AddByTwoPoints(_upperRightPoint2d, _upperLeftPoint2d));
                _rectangleLines.Add(_planarSketch.SketchLines.AddByTwoPoints(_rectangleLines[0].EndSketchPoint, _lowerLeftPoint2d));
                _rectangleLines.Add(_planarSketch.SketchLines.AddByTwoPoints(_rectangleLines[1].EndSketchPoint, _lowerRightPoint2d));
                _rectangleLines.Add(_planarSketch.SketchLines.AddByTwoPoints(_rectangleLines[2].EndSketchPoint,
                                                                            _rectangleLines[0].StartSketchPoint));

                _rectangleLines.ForEach(line => 
                    {
                        line.Construction = _constructionLineButton.Pressed;
                        line.Centerline = _centerLineButton.Pressed;
                    });
            }
            else if (_drawingSketch != null)
            {
                _rectangleLines = new List<SketchLine>();

                _rectangleLines.Add(_drawingSketch.SketchLines.AddByTwoPoints(_upperRightPoint2d, _upperLeftPoint2d));
                _rectangleLines.Add(_drawingSketch.SketchLines.AddByTwoPoints(_rectangleLines[0].EndSketchPoint, _lowerLeftPoint2d));
                _rectangleLines.Add(_drawingSketch.SketchLines.AddByTwoPoints(_rectangleLines[1].EndSketchPoint, _lowerRightPoint2d));
                _rectangleLines.Add(_drawingSketch.SketchLines.AddByTwoPoints(_rectangleLines[2].EndSketchPoint,
                                                                            _rectangleLines[0].StartSketchPoint));

                _rectangleLines.ForEach(i =>
                {
                    i.Centerline = _centerLineButton.Pressed;
                    i.SketchOnly = _sketchOnlyButton.Pressed;
                });
            }
        }

        protected abstract void ConstrainRectangle();

        /// <summary>
        /// This method gets called from the ChangeProcessor and is part of it's encapsulation
        /// </summary>
        /// <param name="Document"></param>
        /// <param name="Context"></param>
        /// <param name="Succeeded"></param>
        public void ChangeProcessor_OnExecute(_Document Document, NameValueMap Context, ref bool Succeeded)
        {
            InstantiateRectangle();
        }
        

        /// <summary>
        /// This method is what cancels the command if the user fires the OnTerminate event.
        /// It's important to reset the variables below to null otherwise the existing variables 
        /// stay in memory and cause problems if the command is called again.
        /// </summary>
        public virtual void StopInteraction()
        {
            if (_interactionEvents != null)
            {
                ResetRectangle();

                _rectangleButton.CommandIsRunning = false;
                _rectangleButton.ButtonPressed = false;
            }
        }

        protected void ResetRectangle()
        {
            if (_mouseEvents != null)
            {
                _mouseEvents.OnMouseMove -= _onMouseMove_Delegate;
                _onMouseMove_Delegate = null;
                _mouseEvents.OnMouseClick -= _onMouseClick_Delegate;
                _onMouseClick_Delegate = null;
                _mouseEvents = null;
            }

            _rectangleLines = null;
            _horizontalMidPointAlign = null;
            _verticalMidPointAlign = null;
            _userInputEvents.OnContextMenu -= _userInputEvents_OnContextMenuDelegate;
            _userInputEvents_OnContextMenuDelegate = null;
            _planarSketch = null;
            _drawingSketch = null;
            _secondSelectedSketchEntity = null;
            _inferredOriginPoint = null;
            _inferredFinalPositionPoint = null;
            _firstSelectedSketchEntity = null;
            _pickedPoint2d = null;
            _pickedSketchPoint = null;
            _rectangleOriginSketchPoint = null;
            _rectangleCoordSet = null;
            _rectanglePointCoords = null;
            _rectangleInteractionGraphics = null;
            _rectangleGraphicsColorSet = null;
            _interactionEvents.OnTerminate -= _onTerminate_Delegate;
            _interactionEvents.Stop();
            _interactionEvents = null;
        }

        /// <summary>
        /// Converts a Point object (which is a 3D Inventor point) into a Point2d object (which is a 
        /// 2D point used in sketches).
        /// </summary>
        /// <param name="modelPosition"></param>
        /// <param name="translatedPoint2d"></param>
        protected void PointToPoint2d(Point modelPosition, out Point2d translatedPoint2d)
        {
            TransientGeometry tG = _inventorApplication.TransientGeometry;
            Point2d tempPoint2d = tG.CreatePoint2d();
            tempPoint2d.X = modelPosition.X;
            tempPoint2d.Y = modelPosition.Y;
            translatedPoint2d = tempPoint2d;
        }
    }
}
