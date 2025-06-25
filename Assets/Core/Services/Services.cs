
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

internal static class Services
{
	static Services()
	{

	}

	internal static TService GetService<TService>()
		where TService :
			class,
			IService
	{
		return Services.c_serviceTypes[
			key: typeof(TService)
		] as TService;
	}

	internal static async Task Start()
	{
		var tasksSetup = new List<Task>();

		// Service Setup
		tasksSetup.AddRange(
			from serviceTypes in Services.c_serviceTypes
				select serviceTypes.Value into service
				select service.Setup()
		);

		await Task.WhenAll(
			tasks: tasksSetup
		);

		// Service Start
		var tasksStart = new List<Task>();

		tasksStart.AddRange(
			from serviceTypes in Services.c_serviceTypes
			select serviceTypes.Value into service
			select service.Start()
		);

		await Task.WhenAll(
			tasks: tasksStart
		);
	}

	internal static async Task Stop()
	{
		var tasksStop = new List<Task>();

		tasksStop.AddRange(
			from serviceTypes in Services.c_serviceTypes
				select serviceTypes.Value into service
				select service.Stop()
		);

		await Task.WhenAll(
			tasks: tasksStop
		);
	}

	private static readonly Dictionary<Type, IService> c_serviceTypes = new()
	{

	};
}