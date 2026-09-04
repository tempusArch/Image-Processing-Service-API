using System;

namespace ImageProcessingServiceAPI.Infrastructure;

public class ForbiddenException : Exception {
    public ForbiddenException(string message) : base(message) {

    }
}

public class NotFoundException : Exception {
    public NotFoundException(string message) : base(message) {
        
    }
}