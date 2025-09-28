import 'package:flutter/material.dart';
import 'package:material_symbols_icons/symbols.dart';
import 'package:popover/popover.dart';

enum HearthIconType { plus, minus }

extension HearthIconTypeExtension on HearthIconType {
  Alignment get alignment {
    return switch (this) {
      HearthIconType.plus => Alignment.centerRight,
      HearthIconType.minus => Alignment.centerLeft
    };
  }

  Icon get icon {
    return switch (this) {
      HearthIconType.plus => Icon(Symbols.heart_plus),
      HearthIconType.minus => Icon(Symbols.heart_minus)
    };
  }

  PopoverDirection get direction {
    return switch (this) {
      HearthIconType.plus => PopoverDirection.right,
      HearthIconType.minus => PopoverDirection.left
    };
  }

  String get label {
    return switch (this) {
      HearthIconType.plus => 'Add HP',
      HearthIconType.minus => 'Remove HP'
    };
  }

  int delta(int value) {
    return switch (this) {
      HearthIconType.plus => value,
      HearthIconType.minus => -value
    };
  }
}
