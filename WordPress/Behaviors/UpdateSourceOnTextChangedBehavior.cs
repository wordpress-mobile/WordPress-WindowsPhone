using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace WordPress.Behaviors
{
    public class UpdateSourceOnTextChangedBehavior:Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.TextChanged += OnAssociatedObjectTextChanged;
        }

        private void OnAssociatedObjectTextChanged(object sender, TextChangedEventArgs args)
        {
            BindingExpression binding = this.AssociatedObject.GetBindingExpression(TextBox.TextProperty);
            if (null != binding)
            {
                binding.UpdateSource();
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            this.AssociatedObject.TextChanged -= OnAssociatedObjectTextChanged;
        }
    }
}
