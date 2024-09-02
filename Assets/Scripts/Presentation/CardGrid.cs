using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation
{
    [RequireComponent(typeof(GridLayoutGroup), typeof(AspectRatioFitter))]
    public class CardGrid : MonoBehaviour
    {
        public void SetSize(int width, int height, float aspectRatio)
        {
            SetGridCellSize(width, height);
            SetAspectRatio(aspectRatio);
        }
        
        private GridLayoutGroup _cardGrid;
        private AspectRatioFitter _aspectRatioFitter;

        private Coroutine _updateAspectRatioCoroutine;
        private Coroutine _updateCellSizeCoroutine;
        
        private int _boardWidth = 1;
        private int _boardHeight = 1;
        private float _aspectRatio = 1f;

        private int BoardWidth
        {
            get => _boardWidth;
            set => _boardWidth = value >= 1 ? value : 1;
        }

        private int BoardHeight
        {
            get => _boardHeight;
            set => _boardHeight = value >= 1 ? value : 1;
        }

        private float AspectRatio
        {
            get => _aspectRatio;
            set => _aspectRatio = value > 0f ? value : 1f;
        }
        
        private void Awake()
        {
            _cardGrid = GetComponent<GridLayoutGroup>();
            _aspectRatioFitter = GetComponent<AspectRatioFitter>();
        }
        
        private void OnRectTransformDimensionsChange()
        {
            if (!gameObject.activeInHierarchy)
                return;

            UpdateGridCellSize();
            UpdateAspectRatio();
        }

        private void SetGridCellSize(int boardWidth, int boardHeight)
        {
            BoardWidth = boardWidth;
            BoardHeight = boardHeight;
            
            UpdateGridCellSize();
        }

        private void UpdateGridCellSize()
        {
            if (!gameObject.activeInHierarchy)
                return;
            
            if (_updateCellSizeCoroutine != null)
                StopCoroutine(_updateCellSizeCoroutine);
            
            _updateCellSizeCoroutine = StartCoroutine(UpdateGridCellSizeCoroutine());
        }

        private IEnumerator UpdateGridCellSizeCoroutine()
        {
            // update immediately
            DoUpdateGridCellSize();

            // skip one frame
            yield return null;

            // update using correct layout values
            DoUpdateGridCellSize();
        }

        private void DoUpdateGridCellSize()
        {
            if (_cardGrid != null)
            {
                var gridRectTransform = _cardGrid.GetComponent<RectTransform>();

                int cellWidth = (int) (gridRectTransform.rect.width
                                       - _cardGrid.padding.left
                                       - _cardGrid.padding.right
                                       - _cardGrid.spacing.x * (BoardWidth - 1))
                                / BoardWidth;

                int cellHeight = (int) (gridRectTransform.rect.height
                                        - _cardGrid.padding.top
                                        - _cardGrid.padding.bottom
                                        - _cardGrid.spacing.y * (BoardHeight - 1))
                                 / BoardHeight;

                _cardGrid.cellSize = new Vector2(cellWidth, cellHeight);
            }
        }
        
        private void SetAspectRatio(float aspectRatio)
        {
            AspectRatio = aspectRatio;
            UpdateAspectRatio();
        }

        private void UpdateAspectRatio()
        {
            if (!gameObject.activeInHierarchy)
                return;
            
            if (_updateAspectRatioCoroutine != null)
                StopCoroutine(_updateAspectRatioCoroutine);
            
            _updateAspectRatioCoroutine = StartCoroutine(UpdateAspectRatioCoroutine());
        }

        private IEnumerator UpdateAspectRatioCoroutine()
        {
            // update immediately
            DoUpdateGridAspectRatio();

            // skip one frame
            yield return null;

            // update using correct layout values
            DoUpdateGridAspectRatio();
        }

        private void DoUpdateGridAspectRatio()
        {
            if (_aspectRatioFitter != null)
            {
                _aspectRatioFitter.aspectRatio = AspectRatio;
                _aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            }
        }
    }
}