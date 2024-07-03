using Godot;

namespace Backrooms.Scripts.Interfaces;

public interface IInteractable
{
    public void Interact(Node3D interacter, params Variant[] args);
}