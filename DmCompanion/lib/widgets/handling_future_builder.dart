import 'package:flutter/material.dart';

class HandlingFutureBuilder<T> extends StatelessWidget {
  final Future<T> future;
  final Widget Function(BuildContext context, T data) builder;
  final bool allowNull;

  const HandlingFutureBuilder(
      {super.key,
      required this.future,
      required this.builder,
      this.allowNull = false});

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<T?>(
        future: future,
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
        });
  }
}
