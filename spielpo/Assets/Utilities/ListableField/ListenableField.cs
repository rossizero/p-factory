using UnityEngine.Events;

namespace ListenableField
{ 
    public class ListenableField<TFieldType>
    {

        public delegate void Notify(TFieldType field);
        public event Notify FieldChanged;

        private TFieldType _field;
        public TFieldType field
        {
            get => _field;
            set
            {
                _field = value;
                FieldChanged?.Invoke(_field);
            }
        }

    }
}