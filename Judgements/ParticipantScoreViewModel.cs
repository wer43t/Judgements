using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judgements
{
    public class ParticipantScoreViewModel : INotifyPropertyChanged
    {
        public int RowNumber { get; set; }
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public float? Score { get; set; }
        public bool CanEdit { get; set; }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEditing)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}


