using MelonLoader.Preferences;

namespace BluePrinceModPreferencesManager;

internal class FloatValidator : ValueValidator
{
    private readonly float defaultValue;
    private readonly float min;
    private readonly float max;
    public FloatValidator(float defaultValue, float min = 0, float max = float.MaxValue) =>
        (this.defaultValue, this.min, this.max) = (defaultValue, min, max);
    public override object EnsureValid(object value) =>
        IsValid(value) ? value : defaultValue;
    public override bool IsValid(object value) =>
        (float)value >= min && (float)value <= max;
}