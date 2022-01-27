using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    void TakeDamage(int Damage);
}
public interface IWorkable
{
    void SetWorker(BaseWorker sender);
}

public interface IDropOff
{
    void RecieveResourse(BaseWorker Worker);
}
public interface ISellable
{
    void SellProperty();
    void TurnOnOff();
}
