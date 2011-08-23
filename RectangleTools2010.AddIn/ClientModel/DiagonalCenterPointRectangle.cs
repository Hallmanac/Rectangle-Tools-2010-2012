using Inventor;
using QubeItTools.ClientConfig;
using QubeItTools.General;
using QubeItTools.Interfaces;

namespace QubeItTools.ClientModel
{
    public class DiagonalCenterPointRectangle : Rectangle, IRectangleLogic, IChangeProcessorParent
    {
        public DiagonalCenterPointRectangle(IInventorButton rectangleButton, Inventor.Application inventorApplication)
        {
            _inventorApplication = inventorApplication;
            _rectangleButton = rectangleButton;
            _changeProcessor = new QubeItToolsChangeProcessor(_inventorApplication, this, 
                "DiagCenterPtRect", "Diagonal Center Point Rectangle", StandardAddInServer.AddInServerId);
            TransientGeometry tG = _inventorApplication.TransientGeometry;
            _upperRightPoint2d = tG.CreatePoint2d();
            _upperLeftPoint2d = tG.CreatePoint2d();
            _lowerLeftPoint2d = tG.CreatePoint2d();
            _lowerRightPoint2d = tG.CreatePoint2d();

            _constructionLineButton = _inventorApplication.CommandManager.ControlDefinitions["SketchConstructionCmd"] as ButtonDefinition;
            _centerLineButton = _inventorApplication.CommandManager.ControlDefinitions["SketchCenterlineCmd"] as ButtonDefinition;
            _sketchOnlyButton = _inventorApplication.CommandManager.ControlDefinitions["SketchOnlyCmd"] as ButtonDefinition;
        }

        /// <summary>
        /// This method positions the Point2d objects in a way to draw the rectangle based on where the _upperRightPoint2d
        /// is located.  It's unique to each type of rectangle created.
        /// </summary>
        protected override void PositionPoint2dObjects()
        {
            _upperLeftPoint2d.X = _upperRightPoint2d.X - ((_upperRightPoint2d.X - _pickedPoint2d.X) * 2);
            _upperLeftPoint2d.Y = _upperRightPoint2d.Y;

            _lowerLeftPoint2d.X = _upperLeftPoint2d.X;
            _lowerLeftPoint2d.Y = _upperLeftPoint2d.Y - ((_upperLeftPoint2d.Y - _pickedPoint2d.Y) * 2);

            _lowerRightPoint2d.X = _upperRightPoint2d.X;
            _lowerRightPoint2d.Y = _lowerLeftPoint2d.Y;
        }

        protected override void DrawRectangle()
        {
            base.DrawRectangle();

            if (_planarSketch != null)
            {
                //The code below would create a diagonal construction line as a different way to draw a center point rectangle
                _rectangleLines.Add(_planarSketch.SketchLines.AddByTwoPoints(_rectangleLines[3].StartSketchPoint,
                _rectangleLines[0].EndSketchPoint));

                _rectangleLines[4].Construction = true;
            }
            else if (_drawingSketch != null)
            {
                //The code below would create a diagonal construction line as a different way to draw a center point rectangle
                _rectangleLines.Add(_drawingSketch.SketchLines.AddByTwoPoints(_rectangleLines[3].StartSketchPoint,
                _rectangleLines[0].EndSketchPoint));

                _rectangleLines[4].Construction = true;
                _rectangleLines[4].SketchOnly = true;
            }
        }

        /// <summary>
        /// Method that takes all the variables created from the user's mouse inferred selection along
        /// with the program created rectangle and constrains it as necessary. It's unique to each type of
        /// rectangle that gets created.
        /// </summary>
        protected override void ConstrainRectangle()
        {
            if (_planarSketch != null)
            {
                //This checks to see if any existing geometry was selected in the first pick and if it was not a point.
                //If there was geometry selected and it wasn't a point, then it creates an origin sketch point for the 
                //rectangle and constrains the origin of the rectangle to the selected sketch entity 
                //(or entities in the case of an intersection inferrence)
                if (_pickedSketchPoint == null && _firstSelectedSketchEntity != null)
                {
                    _rectangleOriginSketchPoint = _planarSketch.SketchPoints.Add(_pickedPoint2d, false);

                    switch (_inferredOriginPoint.InferenceType)
                    {
                        case PointInferenceEnum.kPtAtIntersection:
                            _planarSketch.GeometricConstraints.AddCoincident((SketchEntity)_firstSelectedSketchEntity,
                                (SketchEntity)_rectangleOriginSketchPoint);
                            _planarSketch.GeometricConstraints.AddCoincident((SketchEntity)_firstInferredIntersectedSketchEntity,
                                (SketchEntity)_rectangleOriginSketchPoint);
                            break;
                        case PointInferenceEnum.kPtAtMidPoint:
                            _planarSketch.GeometricConstraints.AddMidpoint(_rectangleOriginSketchPoint,
                                (SketchLine)_firstSelectedSketchEntity);
                            break;
                        case PointInferenceEnum.kPtOnCurve:
                            _planarSketch.GeometricConstraints.AddCoincident((SketchEntity)_firstSelectedSketchEntity,
                                (SketchEntity)_rectangleOriginSketchPoint);
                            break;
                    }
                }

                //This checks to see if there was a sketch entity selected in the second pick. If so, then it
                //sets up the conditions to constrain the corner to the selected sketch entity in the 
                //"ConstrainRectangle()" method.
                if (_inferredFinalPositionPoint != null)
                {
                    switch (_inferredFinalPositionPoint.InferenceType)
                    {
                        case PointInferenceEnum.kPtAtIntersection:
                            _secondSelectedSketchEntity = (SketchEntity)_inferredFinalPositionPoint.Entity[1];
                            _secondInferredIntersectedSketchEntity = (SketchEntity)_inferredFinalPositionPoint.Entity[2];
                            _planarSketch.GeometricConstraints.AddCoincident((SketchEntity)_secondSelectedSketchEntity,
                                   (SketchEntity)_rectangleLines[0].StartSketchPoint);
                            _planarSketch.GeometricConstraints.AddCoincident((SketchEntity)_secondInferredIntersectedSketchEntity,
                                   (SketchEntity)_rectangleLines[0].StartSketchPoint);
                            break;
                        case PointInferenceEnum.kPtOnCurve:
                            _secondSelectedSketchEntity = (SketchEntity)_inferredFinalPositionPoint.Entity[1];
                            _planarSketch.GeometricConstraints.AddCoincident((SketchEntity)_secondSelectedSketchEntity,
                                (SketchEntity)_rectangleLines[0].StartSketchPoint);
                            break;
                        case PointInferenceEnum.kPtOnPt:
                            _secondSelectedSketchEntity = (SketchPoint)_inferredFinalPositionPoint.Entity[1];
                            _rectangleLines[0].StartSketchPoint.Merge((SketchPoint)_secondSelectedSketchEntity);
                            break;
                        case PointInferenceEnum.kPtAtMidPoint:
                            _secondSelectedSketchEntity = (SketchLine)_inferredFinalPositionPoint.Entity[1];
                            _planarSketch.GeometricConstraints.AddMidpoint(_rectangleLines[0].StartSketchPoint,
                                (SketchLine)_secondSelectedSketchEntity);
                            break;
                    }
                }

                /*If the user has selected a random area in space (i.e. no existing geometry) then there won't be a 
                 rectangleOriginSketchPoint so it gets created here.  The commented out code is for when this program
                 creates a center point rectangle by use of a diagonal construction line.*/
                if (_rectangleOriginSketchPoint == null)
                {
                    _rectangleOriginSketchPoint = _planarSketch.SketchPoints.Add(_pickedPoint2d, false);
                }

                _planarSketch.GeometricConstraints.AddMidpoint(_rectangleOriginSketchPoint, _rectangleLines[4]);
                

                //Add the base constraints for the rectangle
                _planarSketch.GeometricConstraints.AddHorizontal((SketchEntity)_rectangleLines[0]);
                _planarSketch.GeometricConstraints.AddPerpendicular((SketchEntity)_rectangleLines[0],
                    (SketchEntity)_rectangleLines[1]);
                _planarSketch.GeometricConstraints.AddPerpendicular((SketchEntity)_rectangleLines[1],
                    (SketchEntity)_rectangleLines[2]);
                _planarSketch.GeometricConstraints.AddPerpendicular((SketchEntity)_rectangleLines[2],
                    (SketchEntity)_rectangleLines[3]);
            }

            else if (_drawingSketch != null)
            {
                //This checks to see if any existing geometry was selected in the first pick and if it was not a point.
                //If there was geometry selected and it wasn't a point, then it creates an origin sketch point for the 
                //rectangle and constrains the origin of the rectangle to the selected sketch entity 
                //(or entities in the case of an intersection inferrence)
                if (_pickedSketchPoint == null && _firstSelectedSketchEntity != null)
                {
                    _rectangleOriginSketchPoint = _drawingSketch.SketchPoints.Add(_pickedPoint2d, false);

                    switch (_inferredOriginPoint.InferenceType)
                    {
                        case PointInferenceEnum.kPtAtIntersection:
                            _drawingSketch.GeometricConstraints.AddCoincident((SketchEntity)_firstSelectedSketchEntity,
                                (SketchEntity)_rectangleOriginSketchPoint);
                            _drawingSketch.GeometricConstraints.AddCoincident((SketchEntity)_firstInferredIntersectedSketchEntity,
                                (SketchEntity)_rectangleOriginSketchPoint);
                            break;
                        case PointInferenceEnum.kPtAtMidPoint:
                            _drawingSketch.GeometricConstraints.AddMidpoint(_rectangleOriginSketchPoint,
                                (SketchLine)_firstSelectedSketchEntity);
                            break;
                        case PointInferenceEnum.kPtOnCurve:
                            _drawingSketch.GeometricConstraints.AddCoincident((SketchEntity)_firstSelectedSketchEntity,
                                (SketchEntity)_rectangleOriginSketchPoint);
                            break;
                    }
                }

                //This checks to see if there was a sketch entity selected in the second pick. If so, then it
                //sets up the conditions to constrain the corner to the selected sketch entity in the 
                //"ConstrainRectangle()" method.
                if (_inferredFinalPositionPoint != null)
                {
                    switch (_inferredFinalPositionPoint.InferenceType)
                    {
                        case PointInferenceEnum.kPtAtIntersection:
                            _secondSelectedSketchEntity = (SketchEntity)_inferredFinalPositionPoint.Entity[1];
                            _secondInferredIntersectedSketchEntity = (SketchEntity)_inferredFinalPositionPoint.Entity[2];
                            _drawingSketch.GeometricConstraints.AddCoincident((SketchEntity)_secondSelectedSketchEntity,
                                   (SketchEntity)_rectangleLines[0].StartSketchPoint);
                            _drawingSketch.GeometricConstraints.AddCoincident((SketchEntity)_secondInferredIntersectedSketchEntity,
                                   (SketchEntity)_rectangleLines[0].StartSketchPoint);
                            break;
                        case PointInferenceEnum.kPtOnCurve:
                            _secondSelectedSketchEntity = (SketchEntity)_inferredFinalPositionPoint.Entity[1];
                            _drawingSketch.GeometricConstraints.AddCoincident((SketchEntity)_secondSelectedSketchEntity,
                                (SketchEntity)_rectangleLines[0].StartSketchPoint);
                            break;
                        case PointInferenceEnum.kPtOnPt:
                            _secondSelectedSketchEntity = (SketchPoint)_inferredFinalPositionPoint.Entity[1];
                            _rectangleLines[0].StartSketchPoint.Merge((SketchPoint)_secondSelectedSketchEntity);
                            break;
                        case PointInferenceEnum.kPtAtMidPoint:
                            _secondSelectedSketchEntity = (SketchLine)_inferredFinalPositionPoint.Entity[1];
                            _drawingSketch.GeometricConstraints.AddMidpoint(_rectangleLines[0].StartSketchPoint,
                                (SketchLine)_secondSelectedSketchEntity);
                            break;
                    }
                }

                /*If the user has selected a random area in space (i.e. no existing geometry) then there won't be a 
                 rectangleOriginSketchPoint so it gets created here.  The commented out code is for when this program
                 creates a center point rectangle by use of a diagonal construction line.*/
                if (_rectangleOriginSketchPoint == null)
                {
                    _rectangleOriginSketchPoint = _drawingSketch.SketchPoints.Add(_pickedPoint2d, false);
                }

                _drawingSketch.GeometricConstraints.AddMidpoint(_rectangleOriginSketchPoint, _rectangleLines[4]);

                _drawingSketch.GeometricConstraints.AddHorizontal((SketchEntity)_rectangleLines[0]);
                _drawingSketch.GeometricConstraints.AddPerpendicular((SketchEntity)_rectangleLines[0],
                    (SketchEntity)_rectangleLines[1]);
                _drawingSketch.GeometricConstraints.AddPerpendicular((SketchEntity)_rectangleLines[1],
                    (SketchEntity)_rectangleLines[2]);
                _drawingSketch.GeometricConstraints.AddPerpendicular((SketchEntity)_rectangleLines[2],
                    (SketchEntity)_rectangleLines[3]);
            }
        }

        
    }
}
