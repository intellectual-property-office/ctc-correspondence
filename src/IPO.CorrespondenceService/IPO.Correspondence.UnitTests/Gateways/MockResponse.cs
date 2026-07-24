using Azure;
using Azure.Core;
using System.Diagnostics.CodeAnalysis;

namespace IPO.Correspondence.UnitTests.Gateways
{
    
    public class MockResponse : Response
    {
        public override int Status => 0;

        public override string ReasonPhrase => string.Empty;

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public override Stream? ContentStream { get; set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public override string ClientRequestId { get; set; } = Guid.NewGuid().ToString();

        [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "<Pending>")]
        public override void Dispose()
        {
            
        }

        protected override bool ContainsHeader(string name)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<HttpHeader> EnumerateHeaders()
        {
            return Enumerable.Empty<HttpHeader>();
        }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        protected override bool TryGetHeader(string name, [NotNullWhen(true)] out string? value)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            throw new NotImplementedException();
        }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        protected override bool TryGetHeaderValues(string name, [NotNullWhen(true)] out IEnumerable<string>? values)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            throw new NotImplementedException();
        }
    }


    
    public class MockResponse<T> : Response<T>
    {
        private readonly T _value;

        public override T Value => _value;

        public MockResponse(T value)
        {
            _value = value;
        }

        public override Response GetRawResponse()
        {
            throw new NotImplementedException();
        }
    }
}
