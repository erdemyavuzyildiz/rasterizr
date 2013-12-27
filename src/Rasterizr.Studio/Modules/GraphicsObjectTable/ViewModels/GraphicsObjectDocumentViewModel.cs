﻿using Caliburn.Micro;
using Gemini.Framework;
using Rasterizr.Pipeline.InputAssembler;
using Rasterizr.Resources;

namespace Rasterizr.Studio.Modules.GraphicsObjectTable.ViewModels
{
    public class GraphicsObjectDocumentViewModel : Document
    {
        private readonly GraphicsObjectViewModel _objectViewModel;

        public object GraphicsObject
        {
            get
            {
                if (_objectViewModel.DeviceChild is InputLayout)
                    return new InputLayoutViewModel((InputLayout) _objectViewModel.DeviceChild);
                if (_objectViewModel.DeviceChild is Buffer)
                    return new BufferViewModel((Buffer) _objectViewModel.DeviceChild);
                return null;
            }
        }

        public GraphicsObjectDocumentViewModel(GraphicsObjectViewModel objectViewModel)
        {
            DisplayName = objectViewModel.Type + " (" + objectViewModel.Identifier + ")";
            _objectViewModel = objectViewModel;
        }
    }
}