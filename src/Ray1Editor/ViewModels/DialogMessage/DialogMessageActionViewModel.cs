using System;
using System.Windows.Input;

namespace Ray1Editor;

/// <summary>
/// View model for a dialog message action
/// </summary>
public class DialogMessageActionViewModel : BaseViewModel
{
    #region Properties

    /// <summary>
    /// The display text
    /// </summary>
    public string DisplayText { get; init; }

    /// <summary>
    /// The display description
    /// </summary>
    public string DisplayDescription { get; init; }

    /// <summary>
    /// The action result
    /// </summary>
    public bool ActionResult { get; init; }

    /// <summary>
    /// True if this is the default action
    /// </summary>
    public bool IsDefault { get; init; }

    /// <summary>
    /// True if this is the default cancel action
    /// </summary>
    public bool IsCancel { get; init; }

    /// <summary>
    /// True if the dialog should close when this action is handled
    /// </summary>
    public bool ShouldCloseDialog { get; init; } = true;

    /// <summary>
    /// Optional action for when this action is handled
    /// </summary>
    public Action OnHandled { get; init; }

    #endregion

    #region Commands

    private ICommand _ActionCommand;

    /// <summary>
    /// Command for when the user selects this action
    /// </summary>
    public ICommand ActionCommand => _ActionCommand ??= new RelayCommand(HandleAction);

    #endregion

    #region Public Methods

    /// <summary>
    /// Handles that the action was chosen by the user
    /// </summary>
    public virtual void HandleAction()
    {
        // Invoke optional action
        OnHandled?.Invoke();
    }

    #endregion
}