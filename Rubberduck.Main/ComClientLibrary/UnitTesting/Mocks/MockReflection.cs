﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ExpressiveReflection;
using Moq;
using Moq.Language;
using Moq.Language.Flow;

namespace Rubberduck.ComClientLibrary.UnitTesting.Mocks
{
    /// <remarks>
    /// Most methods on the <see cref="Mock{T}"/> are generic. Because the <see cref="MethodInfo"/> are
    /// different for each closed generic type, we cannot use the open generic <see cref="MethodInfo"/>.
    /// To address this, we need to get the object, and look up the equivalent <see cref="MethodInfo"/> via
    /// handles which are the same regardless of generic parameters used, and return the "closed" version.
    /// </remarks>
    public static class MockMemberInfos
    {
        public static MethodInfo As()
        {
            return Reflection.GetMethodExt(typeof(Mock), MockMemberNames.As());
        }

        public static MethodInfo Setup(Mock mocked)
        {
            var typeHandle = mocked.GetType().TypeHandle;
            var mock = typeof(Mock<>);
            var expression = typeof(Expression<>).MakeGenericType(typeof(Action<>));
            var genericMethod = Reflection.GetMethodExt(mock, MockMemberNames.Setup(), expression);
            return (MethodInfo) MethodBase.GetMethodFromHandle(genericMethod.MethodHandle, typeHandle);
        }

        public static MethodInfo Returns(object setupMock)
        {
            var typeHandle = setupMock.GetType().GetInterfaces().Single(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IReturns<,>)
            ).TypeHandle;
            var setup = typeof(IReturns<,>);
            var result = typeof(Func<>); 
            var genericMethod = Reflection.GetMethodExt(setup, MockMemberNames.Returns(), result);
            return (MethodInfo) MethodBase.GetMethodFromHandle(genericMethod.MethodHandle, typeHandle);
        }

        public static MethodInfo Callback(object setupMock)
        {
            var typeHandle = setupMock.GetType().GetInterfaces().Single(i =>
                !i.IsGenericType &&
                i == typeof(ICallback)
            ).TypeHandle;
            var setup = typeof(ICallback);
            var callback = typeof(Delegate);
            var genericMethod = Reflection.GetMethodExt(setup, MockMemberNames.Callback(), callback);
            return (MethodInfo) MethodBase.GetMethodFromHandle(genericMethod.MethodHandle, typeHandle);
        }
    }

    /// <remarks>
    /// Though most members are generic, for the purposes of getting the names
    /// they are all the same regardless of the actual closed generic types used
    /// so we can just use object as a placeholder for the generic parameters.
    /// </remarks>
    public static class MockMemberNames
    {
        public static string As()
        {
            return nameof(Mock.As);
        }

        public static string Setup()
        {
            return nameof(Mock<object>.Setup);
        }

        public static string Returns()
        {
            return nameof(ISetup<object, object>.Returns);
        }

        public static string Callback()
        {
            return nameof(ISetup<object>.Callback);
        }
    }
}
