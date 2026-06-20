void logMessage(dynamic sender, String function, String message) {
  print('[$sender] -> [$function] | $message');
}

void logError(dynamic sender, String function, String error) {
  print('[$sender] -> [$function] | ERROR: $error');
}