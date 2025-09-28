import 'package:flutter/material.dart';

class HandlingStreamBuilder<T> extends StatelessWidget {
  final Stream<T> stream;
  final Widget Function(BuildContext context, T data) builder;
  final bool allowNull;

  const HandlingStreamBuilder({
    this.allowNull = false,
    super.key,
    required this.stream,
    required this.builder,
  });

  @override
  Widget build(BuildContext context) {
    return StreamBuilder<T>(
      stream: stream,
      builder: (context, snapshot) {
        if (snapshot.hasError) {
          return Center(
            child: Text('Error: ${snapshot.error}'),
          );
        }
        if (!allowNull && !snapshot.hasData) {
          return Center(
            child: CircularProgressIndicator(),
          );
        }
        return builder(context, snapshot.data as T);
      },
    );
  }
}
