﻿namespace Fuse8.BackendInternship.Domain;

/// <summary>
/// Животное
/// </summary>
public abstract class Animal
{
	/// <summary>
	/// true - если животное является другом человека
	/// </summary>
	public virtual bool IsHumanFriend => false;

	/// <summary>
	/// true - если у животного большой вес
	/// </summary>
	public abstract bool HasBigWeight { get; }

	/// <summary>
	/// Как говорит животное
	/// </summary>
	/// <returns>Возвращает звук, который говорит животное</returns>
	public abstract string WhatDoesSay();
}
// ToDo: В наследниках реализовать метод WhatDoesSay и свойство HasBigWeight, а также переопределить IsHumanFriend там, где это нужно

/// <summary>
/// Собака
/// </summary>
public abstract class Dog : Animal
{
	public override string WhatDoesSay() => "гав";

	public override bool IsHumanFriend => true;
}

/// <summary>
/// Лиса
/// </summary>
public class Fox : Animal
{
	public override string WhatDoesSay() => "ми-ми-ми";

	public override bool HasBigWeight => false;
}

/// <summary>
/// Чихуахуа
/// </summary>
public class Chihuahua : Dog
{
	public override bool HasBigWeight => false;
}

/// <summary>
/// Хаски
/// </summary>
public class Husky : Dog
{
	public new string WhatDoesSay() => "ауф";

	public override bool HasBigWeight => true;
}