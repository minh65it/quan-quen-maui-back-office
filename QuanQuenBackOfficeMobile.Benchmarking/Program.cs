using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using QuanQuenBackOfficeMobile.ViewModels;
using QuanQuenBackOfficeMobile.Services.Interfaces;
using QuanQuenBackOfficeMobile.Models;
using System.Threading.Tasks;

namespace QuanQuenBackOfficeMobile.Benchmarking;

public class Program
{
	// Run this by calling dotnet run -c Release
	public static void Main(string[] args)
	{
		var summary = BenchmarkRunner.Run<Benchmarks>();
	}
}

// Mock implementation of IAuthService for benchmarking
public class MockAuthService : IAuthService
{
	public Task<LoginResponse> LoginAsync(string email, string password)
	{
		return Task.FromResult(new LoginResponse { IsLogin = true, Token = "mock_token" });
	}

	public Task<bool> LogoutAsync()
	{
		return Task.FromResult(true);
	}
}

public class Benchmarks
{
	MainViewModel vmToBenchmark;

	public Benchmarks()
	{
		var mockAuthService = new MockAuthService();
		vmToBenchmark = new MainViewModel(mockAuthService);
	}

	[Benchmark]
	public void Option1() => vmToBenchmark.DoSomething1();

	[Benchmark]
	public void Option2() => vmToBenchmark.DoSomething2();
}
